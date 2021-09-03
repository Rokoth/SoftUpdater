using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SoftUpdaterClient.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace SoftUpdater.UnitTests
{
    public class ClientTests : IClassFixture<ClientCustomFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        public ClientTests(ClientCustomFixture customFixture)
        {
            _serviceProvider = customFixture.ServiceProvider;
        }

        [Fact]
        public async Task BackupServiceTest()
        {
            string dbName = $"soft_updater_client_test_{DateTimeOffset.Now:yyyy_MM_dd_HH_mm_ss}";
            var connectionString = $"Server=localhost;Database={dbName};Username=postgres;Password=Rok_Oth_123";
            var rootConnectionString = Regex.Replace(connectionString, "Database=.*?;", $"Database=postgres;");
            var appDir = Path.Combine(Directory.GetCurrentDirectory(), "testAppPath");
            var backUpDir = Path.Combine(Directory.GetCurrentDirectory(), "testbackUpPath");
            try
            {              
                var backUpService = _serviceProvider.GetRequiredService<IBackupService>();                            

                var ignoreDirectories = new List<string> { "ignore_dir_1", "ignore_dir_2", "ignore_dir_3" };
                var ignoreFiles = new List<string> { "ignore_file_1.txt", "ignore_file_2.txt", "ignore_file_3.txt" };

                Directory.CreateDirectory(appDir);
                Directory.CreateDirectory(backUpDir);

                var toCreateFiles = new List<string> { "backUp_file_1.txt", "backUp_file_2.txt", "backUp_file_3.txt" };
                var toCreateDirectories = new List<string> { "backUp_dir_1", "backUp_dir_2", "backUp_dir_3" };
                toCreateFiles.AddRange(ignoreFiles);
                toCreateDirectories.AddRange(ignoreDirectories);

                foreach (var dir in toCreateDirectories)
                {
                    Directory.CreateDirectory(Path.Combine(appDir, dir));
                }

                foreach (var fileName in toCreateFiles)
                {
                    using (var streamWriter = new StreamWriter(Path.Combine(appDir, fileName)))
                    {
                        streamWriter.WriteLine("test");
                    }
                }
                CreateDabase(rootConnectionString, connectionString, dbName);

                var result = await backUpService.Backup(appDir, backUpDir, new string[] { connectionString }, ignoreDirectories, ignoreFiles);
                Assert.True(result);
                var backUpDbFileExists = Directory.GetFiles(backUpDir, $"backup_*.backup").Any();
                Assert.True(backUpDbFileExists);

                var directories = Directory.GetDirectories(backUpDir);
                foreach (var dir in directories)
                {
                    var dirName = dir.Split(Path.DirectorySeparatorChar).Last();
                    Assert.Contains(dirName, toCreateDirectories);
                    Assert.DoesNotContain(dirName, ignoreDirectories);
                }

                var files = Directory.GetFiles(backUpDir);
                foreach (var file in files)
                {
                    var fileName = file.Split(Path.DirectorySeparatorChar).Last();
                    if (!fileName.EndsWith("backup"))
                    {
                        Assert.Contains(fileName, toCreateFiles);
                        Assert.DoesNotContain(fileName, ignoreFiles);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if(Directory.Exists(appDir)) Directory.Delete(appDir, true);
                if (Directory.Exists(backUpDir)) Directory.Delete(backUpDir, true);
                DropDatabase(rootConnectionString, dbName);
            }
        }

        [Fact]
        public void UpdateScriptParserTest()
        {
            var script = "A1: Stop\r\nA2: (A1) Backup\r\nA3: (A2) CMD \"7z x \\\"{{ReleasePath}}\\\\app.zip\\\" -o\\\"{{ReleasePath}}\\\"\"\r\nA5: (A2 and A3) Install\r\nA6: (not A5) RollBack\r\nA7: (A5 or A6) Start\r\n";
            var parseService = _serviceProvider.GetRequiredService<IUpdateScriptParser>();
            var result = parseService.Parse(script.Split("\r\n"));
            Assert.Equal("A1", result[0].Name);
            Assert.Equal(CommandEnum.Stop, result[0].CommandType);
            Assert.Null(result[0].Arguments);            

            Assert.Equal("A2", result[1].Name);
            Assert.Equal(CommandEnum.Backup, result[1].CommandType);
            Assert.Null(result[1].Arguments);

            Assert.Equal("A3", result[2].Name);
            Assert.Equal(CommandEnum.CMD, result[2].CommandType);
            Assert.NotEmpty(result[2].Arguments);

            Assert.Equal("A5", result[3].Name);
            Assert.Equal(CommandEnum.Install, result[3].CommandType);
            Assert.Null(result[3].Arguments);

            Assert.Equal("A6", result[4].Name);
            Assert.Equal(CommandEnum.Rollback, result[4].CommandType);
            Assert.Null(result[4].Arguments);

            Assert.Equal("A7", result[5].Name);
            Assert.Equal(CommandEnum.Start, result[5].CommandType);
            Assert.Null(result[5].Arguments);
        }

        private void CreateDabase(string rootConnectionString, string connectionString, string dbName)
        {
            try
            {
                using (NpgsqlConnection _connPg = new NpgsqlConnection(rootConnectionString))
                {
                    _connPg.Open();
                    string script1 = $"select exists(SELECT 1 FROM pg_database WHERE datname = '{dbName}');";
                    var cmd1 = new NpgsqlCommand(script1, _connPg);
                    if (!(bool)cmd1.ExecuteScalar())
                    {
                        string script2 = $"create database {dbName};";
                        var cmd2 = new NpgsqlCommand(script2, _connPg);
                        cmd2.ExecuteNonQuery();                        
                    }
                }

                using (NpgsqlConnection _connPg = new NpgsqlConnection(connectionString))
                {
                    _connPg.Open();
                    string script3 = $"create table test(id serial, mess varchar);";
                    var cmd3 = new NpgsqlCommand(script3, _connPg);
                    cmd3.ExecuteNonQuery();

                    string script4 = $"insert into test(mess) values ('test text');";
                    var cmd4 = new NpgsqlCommand(script4, _connPg);
                    cmd4.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось развернуть базу данных: " +
                    $"ошибка при проверке или создании базы: {ex.Message} {ex.StackTrace}");
            }
        }

        private void DropDatabase(string connectionString, string databaseName)
        {
            try
            {
                using NpgsqlConnection _connPg = new NpgsqlConnection(connectionString);
                _connPg.Open();
                string script1 = "SELECT pg_terminate_backend (pg_stat_activity.pid) " +
                    $"FROM pg_stat_activity WHERE pid<> pg_backend_pid() AND pg_stat_activity.datname = '{databaseName}'; ";
                var cmd1 = new NpgsqlCommand(script1, _connPg);
                cmd1.ExecuteNonQuery();

                string script2 = $"DROP DATABASE {databaseName};";
                var cmd2 = new NpgsqlCommand(script2, _connPg);
                cmd2.ExecuteNonQuery();
            }
            catch
            {

            }
        }
    }
}
