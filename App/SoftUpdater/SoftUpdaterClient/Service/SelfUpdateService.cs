using DbClient.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUpdater.ClientHttpClient;
using SoftUpdater.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public class SelfUpdateService : ISelfUpdateService
    {
        private IServiceProvider _serviceProvider;
        private ILogger _logger;
        private IServiceHelper _serviceHelper;
        private ISelfBackupService _backupService;
        private IClientHttpClient httpClient;
        private ClientOptions _options;
        private DbSqLiteContext _context;
        private string _downloadedVersionField;
        private string _installedVersionField;
        private IInstallSelfService _installService;
        private IRollBackService _rollBackService;
        private IUpdateScriptParser _scriptParser;

        public SelfUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<InstallService>>();
            _serviceHelper = _serviceProvider.GetRequiredService<IServiceHelper>();
            _backupService = _serviceProvider.GetRequiredService<ISelfBackupService>();
            httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
            _options = _serviceProvider.GetRequiredService<IOptions<ClientOptions>>().Value;
            _context = _serviceProvider.GetRequiredService<DbSqLiteContext>();
            _installService = _serviceProvider.GetRequiredService<IInstallSelfService>();
            _rollBackService = _serviceProvider.GetRequiredService<IRollBackService>();
            _scriptParser = _serviceProvider.GetRequiredService<IUpdateScriptParser>();

            _downloadedVersionField = _options.DownloadedSelfVersionField;
            _installedVersionField = _options.InstalledSelfVersionField;
        }

        public async Task Execute()
        {            
            bool success = true;
            var script = _options.UpdateSelfScript;
            var commands = _scriptParser.Parse(script.Split("\r\n"));

            foreach (var command in commands)
            {
                if (command.Condition.GetResult(commands))
                {
                    switch (command.CommandType)
                    {
                        case CommandEnum.Backup:
                            if (!await _backupService.Backup(
                                _options.ApplicationSelfDirectory, 
                                _options.BackupSelfDirectory,
                                new string[] { }, 
                                new List<string>() {
                                    _options.BackupSelfDirectory,
                                    _options.ReleasePathSelf, 
                                    Directory.GetCurrentDirectory()
                                }, new List<string>())) success = false;
                            break;
                        case CommandEnum.CMD:
                            if (!_serviceHelper.ExecuteCommand(command.Name + string.Join(" ", command.Arguments))) success = false;
                            break;
                        case CommandEnum.Install:
                            if (!_installService.Install(new InstallSettings()
                            {
                                AppDir = _options.ApplicationSelfDirectory,
                                BackupDir = _options.BackupSelfDirectory,
                                DoBackup = true,
                                IgnoreDirectories = new List<string>() { 
                                    _options.BackupSelfDirectory, 
                                    _options.ReleasePathSelf, 
                                    Directory.GetCurrentDirectory() 
                                },
                                IgnoreFiles = new List<string>(),
                                InstallType = InstallType.Replace,
                                TmpDir = _options.ReleasePathSelf
                            })) success = false;
                            break;
                        case CommandEnum.Rollback:
                            await _rollBackService.RollBack(
                                _options.ApplicationSelfDirectory, 
                                _options.BackupSelfDirectory,
                                new[] { _options.ConnectionString }, 
                                new List<string>() {
                                    _options.BackupSelfDirectory, 
                                    _options.ReleasePathSelf, 
                                    Directory.GetCurrentDirectory()
                                }, new List<string>());
                            break;
                        case CommandEnum.Start:
                            if (!_serviceHelper.ExecuteCommand($"service {_options.ServiceName} start")) success = false;
                            break;
                        case CommandEnum.Stop:
                            if (!_serviceHelper.ExecuteCommand($"service {_options.ServiceName} stop")) success = false;
                            break;
                    }
                }
            }

            if (success)
            {
                var downloadedVersion = _context.Settings.FirstOrDefault(s => s.ParamName == _downloadedVersionField);
                var installedVersion = _context.Settings.FirstOrDefault(s => s.ParamName == _installedVersionField);
                if (installedVersion != null)
                {
                    installedVersion.ParamValue = downloadedVersion.ParamValue;
                    _context.Settings.Update(installedVersion);
                }
                else
                {
                    var maxId = _context.Settings.Select(s => s.Id).Max();
                    installedVersion = new DbClient.Model.Settings()
                    {
                        Id = maxId + 1,
                        ParamName = _installedVersionField,
                        ParamValue = downloadedVersion.ParamValue
                    };
                    _context.Settings.Add(installedVersion);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogError("Не удалось обновить сервис");
            }
        }
    }
}
