using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SoftUpdaterClient.Service
{
    public class InstallService
    {
        private IServiceProvider _serviceProvider;
        private ILogger _logger;
        private IServiceHelper _serviceHelper;

        public InstallService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<InstallService>>();
            _serviceHelper = _serviceProvider.GetRequiredService<IServiceHelper>();
        }

        public bool Install(InstallType installType, string tmpDir, string appDir, string backupDir)
        {
            try
            {
                switch (installType)
                {
                    case InstallType.Replace:
                        _serviceHelper.DirectoryCopy(tmpDir, appDir, true);
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
                        _serviceHelper.ExecuteCommand(command);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при установке обновления: {ex.Message} {ex.StackTrace}");
                if (!RollBack(appDir, backupDir))
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
                _serviceHelper.DirectoryCopy(backupDir, appDir, true);
                return true;
            }
            catch (Exception ex)
            {
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
}
