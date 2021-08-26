using Cronos;
using DbClient.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUpdater.ClientHttpClient;
using SoftUpdater.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public class InstallHostedService : IHostedService
    {
        private IServiceProvider _serviceProvider;
        private CancellationTokenSource _cancellationTokenSource;
        private DateTimeOffset _nextRunDateTime;
        private DbSqLiteContext _context;
        private readonly CronExpression _expression;
        private string _nextRunDateTimeField;
        private string _downloadedVersionField;
        private string _installedVersionField;
        private ClientOptions _options;
        private ILogger _logger;
        private IClientHttpClient httpClient;

        public InstallHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = new CancellationTokenSource();
            _context = _serviceProvider.GetRequiredService<DbSqLiteContext>();
            _options = _serviceProvider.GetRequiredService<IOptions<ClientOptions>>().Value;
            _downloadedVersionField = _options.DownloadedVersionField;
            _installedVersionField = _options.InstalledVersionField;
            _nextRunDateTimeField = _options.NextRunDateTimeField;
            _expression = CronExpression.Parse(_options.InstallSchedule);
            _logger = _serviceProvider.GetRequiredService<ILogger<InstallHostedService>>();
            httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
            GetNextRunDateTime();
        }

        private async Task Run()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_nextRunDateTime <= DateTimeOffset.Now)
                {
                    try
                    {
                        
                        var downloadedVersion = _context.Settings.FirstOrDefault(s => s.ParamName == _downloadedVersionField);
                        var installedVersion = _context.Settings.FirstOrDefault(s => s.ParamName == _installedVersionField);                        
                        if (VersionCompare(downloadedVersion?.ParamValue, installedVersion?.ParamValue))
                        {
                            var installService = _serviceProvider.GetRequiredService<IInstallService>();
                            var backUpService = _serviceProvider.GetRequiredService<IBackupService>();
                            var rollBackService = _serviceProvider.GetRequiredService<IRollBackService>();
                            var scriptParser = _serviceProvider.GetRequiredService<IUpdateScriptParser>();
                            var _serviceHelper = _serviceProvider.GetRequiredService<IServiceHelper>();
                            bool success = true;
                            var script = _options.UpdateScript;
                            var commands = scriptParser.Parse(script.Split("\r\n"));

                            foreach (var command in commands)
                            {
                                if (command.Condition.GetResult(commands))
                                {
                                    switch (command.CommandType)
                                    {
                                        case CommandEnum.Backup:
                                            if(!await backUpService.Backup(_options.ApplicationDirectory, _options.BackupDirectory, 
                                                new[] { _options.ConnectionString }, new List<string>() {
                                                   _options.BackupDirectory, _options.ReleasePath, Directory.GetCurrentDirectory()
                                                }, new List<string>())) success = false;
                                            break;
                                        case CommandEnum.CMD:
                                            if(!_serviceHelper.ExecuteCommand(command.Name + string.Join(" ", command.Arguments))) success = false;
                                            break;
                                        case CommandEnum.Install:
                                            if(!installService.Install(new InstallSettings()
                                            {
                                                AppDir = _options.ApplicationDirectory,
                                                BackupDir = _options.BackupDirectory,
                                                DoBackup = true,
                                                IgnoreDirectories = new List<string>() {
                                                       _options.BackupDirectory, _options.ReleasePath, Directory.GetCurrentDirectory()
                                                    },
                                                IgnoreFiles = new List<string>(),
                                                InstallType = InstallType.Replace,
                                                TmpDir = _options.ReleasePath
                                            })) success = false;
                                            break;
                                        case CommandEnum.Rollback:
                                            await rollBackService.RollBack(_options.ApplicationDirectory, _options.BackupDirectory,
                                                new[] { _options.ConnectionString }, new List<string>() {
                                                   _options.BackupDirectory, _options.ReleasePath, Directory.GetCurrentDirectory()
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
                    catch (Exception ex)
                    {
                        await httpClient.SendErrorMessage($"Ошибка при обновлении сервиса: {ex.Message} {ex.StackTrace}");
                        _logger.LogError($"Ошибка при обновлении сервиса: {ex.Message} {ex.StackTrace}");
                    }
                    finally
                    {
                        GetNextRunDateTime();
                    }
                }
            }
        }

        private bool VersionCompare(string downloadedVersion, string installedVersion)
        {
            try
            {
                if (string.IsNullOrEmpty(downloadedVersion)) return false;
                if (string.IsNullOrEmpty(installedVersion)) return true;
                if (downloadedVersion == installedVersion) return false;
                var downLoaded = downloadedVersion.Split('.').Select(s => int.Parse(s)).ToArray();
                var installed = installedVersion.Split('.').Select(s => int.Parse(s)).ToArray();
                 
                for (int i = 0; i < downLoaded.Length; i++)
                {
                    if (installed.Length < i) return true;
                    if(downLoaded[i] > installed[i]) return true;
                    if (downLoaded[i] < installed[i]) return false;
                }
                return false;
            }
            catch
            {
                return downloadedVersion != installedVersion;
            }
        }

        private void GetNextRunDateTime()
        {
            if (_nextRunDateTime == null)
            {
                var nextRunDateTimeSettings = _context.Settings.FirstOrDefault(s => s.ParamName == _nextRunDateTimeField);
                if (nextRunDateTimeSettings != null)
                {
                    if (DateTimeOffset.TryParse(nextRunDateTimeSettings.ParamValue, out _nextRunDateTime)) return;
                }                
            }
            _nextRunDateTime = _expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local).Value;
            var nextRunDateTime = _context.Settings.FirstOrDefault(s => s.ParamName == _nextRunDateTimeField);
            if (nextRunDateTime != null)
            {
                nextRunDateTime.ParamValue = _nextRunDateTime.ToString();
                _context.Settings.Update(nextRunDateTime);                
            }
            else
            {
                var maxId = _context.Settings.Select(s => s.Id).Max();
                _context.Settings.Add(new DbClient.Model.Settings() { 
                   Id = maxId + 1,
                   ParamName = _nextRunDateTimeField,
                    ParamValue = _nextRunDateTime.ToString()
                    });
            }
            _context.SaveChanges();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            await Task.CompletedTask;
        }
    }
}
