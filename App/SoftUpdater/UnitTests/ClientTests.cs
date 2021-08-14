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

namespace TaskCollector.UnitTests
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
                CreateDabase(rootConnectionString, dbName);

                var result = await backUpService.Backup(appDir, backUpDir, new string[] { connectionString }, ignoreDirectories, ignoreFiles);
                Assert.True(result);
                var backUpDbFileExists = Directory.GetFiles(backUpDir, $"backup_*.backup").Any();
                Assert.True(backUpDbFileExists);
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

        private void CreateDabase(string rootConnectionString, string dbName)
        {
            try
            {                                
                using NpgsqlConnection _connPg = new NpgsqlConnection(rootConnectionString);
                _connPg.Open();
                string script1 = $"select exists(SELECT 1 FROM pg_database WHERE datname = '{dbName}');";
                var cmd1 = new NpgsqlCommand(script1, _connPg);
                if (!(bool)cmd1.ExecuteScalar())
                {
                    string script2 = $"create database {dbName};use {dbName};";
                    var cmd2 = new NpgsqlCommand(script2, _connPg);
                    cmd2.ExecuteNonQuery();

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
