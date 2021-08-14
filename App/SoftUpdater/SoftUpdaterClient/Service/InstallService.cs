using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.ClientHttpClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SoftUpdaterClient.Service
{
    public class InstallService : IInstallService
    {
        private IServiceProvider _serviceProvider;
        private ILogger _logger;
        private IServiceHelper _serviceHelper;
        private IBackupService _backupService;
        private IClientHttpClient httpClient;

        public InstallService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<InstallService>>();
            _serviceHelper = _serviceProvider.GetRequiredService<IServiceHelper>();
            _backupService = _serviceProvider.GetRequiredService<IBackupService>();
            httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
        }

        public bool Install(InstallSettings settings)
        {
            try
            {
                switch (settings.InstallType)
                {
                    case InstallType.Replace:
                        _serviceHelper.DirectoryCopy(settings.TmpDir, settings.AppDir, settings.IgnoreDirectories, settings.IgnoreFiles, true);
                        break;
                    case InstallType.Setup:
                        var setUpFile = Directory.GetFiles(settings.TmpDir, "*.msi").FirstOrDefault();
                        string command = null;
                        if (setUpFile != null)
                        {
                            command = $"MSIEXEC /I \"{setUpFile}\" /L*v \"{settings.TmpDir}\\install.log\"";
                        }
                        else
                        {
                            setUpFile = Directory.GetFiles(settings.TmpDir, "*.exe").FirstOrDefault();
                            if (setUpFile != null)
                            {
                                command = $"\"{setUpFile}\" /L*v \"{settings.TmpDir}\\install.log\" /I";
                            }
                        }
                        if (command == null) throw new Exception($"Файл установки в директории {settings.TmpDir} не найден");
                        _serviceHelper.ExecuteCommand(command);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                httpClient.SendErrorMessage($"Ошибка при установке: {ex.Message} {ex.StackTrace}");
                _logger.LogError($"Ошибка при установке обновления: {ex.Message} {ex.StackTrace}");
                if (!RollBack(settings.AppDir, settings.BackupDir))
                {
                    throw new Exception("Не удалось сделать откат после сбоя установки");
                }
                return false;
            }
        }

        private bool RollBack(string appDir, string backupDir)
        {
            try
            {
                _serviceHelper.DirectoryCopy(backupDir, appDir, new List<string>(), new List<string>(), true);
                return true;
            }
            catch (Exception ex)
            {
                httpClient.SendErrorMessage($"Ошибка при резервном копировании: {ex.Message} {ex.StackTrace}");
                _logger.LogError($"Ошибка при установке обновления: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }
    }

    public enum InstallType
    { 
        Setup = 1,
        Replace = 2
    }

    public class InstallSettings
    { 
        public InstallType InstallType { get; set; }
        public string TmpDir { get; set; }
        public string AppDir { get; set; }
        public string BackupDir { get; set; }
        public bool DoBackup { get; set; }
        public List<string> IgnoreDirectories { get; set; }
        public List<string> IgnoreFiles { get; set; }
    }
}
