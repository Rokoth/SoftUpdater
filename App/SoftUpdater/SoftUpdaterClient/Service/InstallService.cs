using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public class InstallService
    {
        IServiceProvider _serviceProvider;
        ILogger _logger;

        public InstallService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<InstallService>>();
        }

        public bool Install(InstallType installType, string tmpDir, string appDir, string backupDir)
        {
            try
            {
                switch (installType)
                {
                    case InstallType.Replace:
                        DirectoryCopy(tmpDir, appDir, true);
                        break;
                    case InstallType.Setup:
                        var setUpFile = Directory.GetFiles(tmpDir, "*.msi").FirstOrDefault();
                        string command = null;
                        if (setUpFile != null)
                        {
                            command = $"MSIEXEC /I \"{setUpFile}\" /L*v \"{tmpDir}\\install.log\"";
                        }
                        else
                        {
                            setUpFile = Directory.GetFiles(tmpDir, "*.exe").FirstOrDefault();
                            if (setUpFile != null)
                            {
                                command = $"\"{setUpFile}\" /L*v \"{tmpDir}\\install.log\" /I";
                            }
                        }
                        if (command == null) throw new Exception($"Файл установки в директории {tmpDir} не найден");
                        ExecuteCommand(command);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при установке обновления: {ex.Message} {ex.StackTrace}");
                if (!Backup(appDir, backupDir))
                {
                    throw new Exception("Не удалось сделать откат после сбоя установки");
                }
                return false;
            }
        }

        private void ExecuteCommand(string command)
        {
            Process p = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @"/c " + command // cmd.exe spesific implementation
            };
            p.StartInfo = startInfo;
            p.Start();
        }

        private bool Backup(string appDir, string backupDir)
        {
            try
            {
                DirectoryCopy(backupDir, appDir, true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при установке обновления: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
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
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }

    public enum InstallType
    { 
        Setup = 1,
        Replace = 2
    }
}
