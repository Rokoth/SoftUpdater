using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.ClientHttpClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public class ServiceHelper : IServiceHelper
    {
        private IServiceProvider _serviceProvider;
        private ILogger _logger;
        private IClientHttpClient httpClient;

        public ServiceHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<ServiceHelper>>();
            httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
        }
               
        public bool DirectoryCopy(string sourceDirName, string destDirName, List<string> ignoreDirectories, List<string> ignoreFiles, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();

                // If the destination directory doesn't exist, create it.       
                Directory.CreateDirectory(destDirName);

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (!ignoreFiles.Contains(file.Name))
                    {
                        string tempPath = Path.Combine(destDirName, file.Name);
                        file.CopyTo(tempPath, false);
                    }
                }

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        if (!ignoreDirectories.Contains(subdir.Name))
                        {
                            string tempPath = Path.Combine(destDirName, subdir.Name);
                            DirectoryCopy(subdir.FullName, tempPath, ignoreDirectories, ignoreFiles, copySubDirs);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                httpClient.SendErrorMessage($"Ошибка DirectoryCopy: {ex.Message} {ex.StackTrace}");
                _logger.LogError($"Ошибка при копировании каталога: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }

        public Dictionary<string, string> Parse(string connectionString)
        {
            var result = new Dictionary<string, string>();
            var parts = connectionString.Split(";");
            foreach (var part in parts)
            {
                var items = part.Split("=");
                result.Add(items[0], items[1]);
            }
            return result;
        }

        public bool ExecuteCommand(string command)
        {
            try
            {
                Process p = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = @"/c " + command // cmd.exe spesific implementation
                };
                p.StartInfo = startInfo;
                p.Start();
                return true;
            }
            catch (Exception ex)
            {
                httpClient.SendErrorMessage($"Ошибка ExecuteCommand: {ex.Message} {ex.StackTrace}");
                _logger.LogError($"Ошибка при выполнении команды: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }

        private readonly string Set = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "set " : "export ";

        public async Task<bool> PostgreSqlRestore(string inputFile, string host, string port, string database, string user, string password)
        {
            string dumpCommand = $"{Set}PGPASSWORD={password}\n" +
                                 $"psql -h {host} -p {port} -U {user} -d {database} -c \"select pg_terminate_backend(pid) from pg_stat_activity where datname = '{database}'\"\n" +
                                 $"dropdb -h " + host + " -p " + port + " -U " + user + $" {database}\n" +
                                 $"createdb -h " + host + " -p " + port + " -U " + user + $" {database}\n" +
                                 "pg_restore -h " + host + " -p " + port + " -d " + database + " -U " + user + "";

            dumpCommand = $"{dumpCommand} {inputFile}";

            return await Execute(dumpCommand);
        }

        public Task<bool> Execute(string command)
        {
            return Task.Run(() =>
            {
                string batFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}." + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bat" : "sh"));
                try
                {
                    string batchContent = "";
                    batchContent += $"{command}";

                    File.WriteAllText(batFilePath, batchContent, Encoding.ASCII);

                    ProcessStartInfo info = ProcessInfoByOS(batFilePath);

                    using Process proc = Process.Start(info);

                    proc.WaitForExit();
                    var exit = proc.ExitCode;
                    proc.Close();                    
                    return true;
                }
                catch (Exception ex)
                {
                    httpClient.SendErrorMessage($"Ошибка при выполнении команды: {ex.Message} {ex.StackTrace}");
                    _logger.LogError($"Ошибка при выполнении команды: {ex.Message} {ex.StackTrace}");
                    return false;
                }
                finally
                {
                    try
                    {
                        if (File.Exists(batFilePath)) File.Delete(batFilePath);
                    }
                    catch 
                    {                        
                    }
                }
            });
        }

        private ProcessStartInfo ProcessInfoByOS(string batFilePath)
        {
            ProcessStartInfo info;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                info = new ProcessStartInfo(batFilePath)
                {
                };
            }
            else
            {
                info = new ProcessStartInfo("sh")
                {
                    Arguments = $"{batFilePath}"
                };
            }

            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            info.RedirectStandardError = true;

            return info;
        }

        public async Task<bool> PostgreSqlDump(string outFile, string host, string port, string database,
            string user,
            string password)
        {
            string dumpCommand =
                 $"{Set}PGPASSWORD={password}\n" +
                 $"pg_dump" + " -Fc" + " -h " + host + " -p " + port + " -d " + database + " -U " + user + "";

            string batchContent = "" + dumpCommand + "  > " + "\"" + outFile + "\"" + "\n";
            if (File.Exists(outFile)) File.Delete(outFile);

            return await Execute(batchContent);
        }

        
    }
}
