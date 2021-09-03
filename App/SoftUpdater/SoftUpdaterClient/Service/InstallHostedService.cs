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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public class InstallHostedService : IHostedService
    {
        private IServiceProvider _serviceProvider;
        private CancellationTokenSource _cancellationTokenSource;
        private DateTimeOffset _nextRunDateTime;
        
        private readonly CronExpression _expression;
        private string _nextRunDateTimeField;
        private string _downloadedVersionField;
        private string _installedVersionField;
        private ClientOptions _options;
        private ILogger _logger;
        

        public InstallHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = new CancellationTokenSource();
           
            _options = _serviceProvider.GetRequiredService<IOptions<ClientOptions>>().Value;
            _downloadedVersionField = _options.DownloadedVersionField;
            _installedVersionField = _options.InstalledVersionField;
            _nextRunDateTimeField = _options.NextRunDateTimeField;
            _expression = CronExpression.Parse(_options.InstallSchedule, CronFormat.IncludeSeconds);
            _logger = _serviceProvider.GetRequiredService<ILogger<InstallHostedService>>();
            
            
        }

        private async Task Run()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<DbSqLiteContext>();
                var httpClient = scope.ServiceProvider.GetRequiredService<IClientHttpClient>();
                GetNextRunDateTime(_context);
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
                                var installService = scope.ServiceProvider.GetRequiredService<IInstallService>();
                                var backUpService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                                var rollBackService = scope.ServiceProvider.GetRequiredService<IRollBackService>();
                                var scriptParser = scope.ServiceProvider.GetRequiredService<IUpdateScriptParser>();
                                var _serviceHelper = scope.ServiceProvider.GetRequiredService<IServiceHelper>();
                                bool success = true;
                                var script = _options.UpdateScript;
                                var commands = scriptParser.Parse(script.Split("\r\n"));

                                var connStrings = _options.ConnectionStrings.Where(s => s.Key != "LocalConnection").Select(s => s.Value).ToArray();

                                foreach (var command in commands)
                                {
                                    if (command.Condition.GetResult(commands))
                                    {
                                        switch (command.CommandType)
                                        {
                                            case CommandEnum.Backup:
                                                if (!await backUpService.Backup(_options.ApplicationDirectory, _options.BackupDirectory,
                                                    connStrings, new List<string>() {
                                                   _options.BackupDirectory, _options.ReleasePath, Directory.GetCurrentDirectory()
                                                    }, new List<string>())) success = false;
                                                break;
                                            case CommandEnum.CMD:
                                                var arguments = new List<string>();
                                                foreach (var arg in command.Arguments)
                                                {
                                                    var newArg = arg;
                                                    if (newArg.Contains("{{"))
                                                    {
                                                        var field = Regex.Match(newArg, "({{.*?}}) /G", RegexOptions.IgnoreCase);
                                                        var prop = typeof(ClientOptions).GetProperties().Where(s => s.Name.Equals(field.Groups[1].Value)).FirstOrDefault();
                                                        if (prop != null)
                                                        {
                                                            newArg.Replace($"{{{prop.Name}}}", prop.GetValue(_options).ToString());
                                                        }
                                                    }
                                                    arguments.Add(newArg);
                                                }
                                                if (!_serviceHelper.ExecuteCommand(command.Name + string.Join(" ", command.Arguments))) success = false;
                                                break;
                                            case CommandEnum.Install:
                                                if (!installService.Install(new InstallSettings()
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
                                                    connStrings, new List<string>() {
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
                                    SaveSettings(_context, downloadedVersion.ParamValue, _installedVersionField);                                    
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
                            GetNextRunDateTime(_context);
                        }
                    }
                }
            }
        }

        private DbClient.Model.Settings SaveSettings(DbSqLiteContext _context, string value, string fieldName)
        {
            var currentSettings = _context.Settings.FirstOrDefault(s => s.ParamName == fieldName);
            if (currentSettings != null)
            {
                currentSettings.ParamValue = value;
                _context.Settings.Update(currentSettings);
                _context.SaveChanges();
            }
            else
            {
                var maxId = 1;
                if (_context.Settings.Any())
                {
                    maxId = _context.Settings.Max(s => s.Id) + 1;
                }
                currentSettings = new DbClient.Model.Settings()
                {
                    Id = maxId,
                    ParamName = fieldName,
                    ParamValue = value
                };
                _context.Settings.Add(currentSettings);
                _context.SaveChanges();
            }

            return currentSettings;
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

        private void GetNextRunDateTime(DbSqLiteContext _context)
        {
            var nextRunDateTimeSettings = _context.Settings.FirstOrDefault(s => s.ParamName == _nextRunDateTimeField);
            if (_nextRunDateTime == null)
            {
                if (nextRunDateTimeSettings != null)
                {
                    if (DateTimeOffset.TryParse(nextRunDateTimeSettings.ParamValue, out _nextRunDateTime)) return;
                }
                _nextRunDateTime = DateTimeOffset.Now;
            }
            _nextRunDateTime = _expression.GetNextOccurrence(_nextRunDateTime, TimeZoneInfo.Local).Value;
            if (nextRunDateTimeSettings != null)
            {
                nextRunDateTimeSettings.ParamValue = _nextRunDateTime.ToString();
                _context.Settings.Update(nextRunDateTimeSettings);
                _context.SaveChanges();
            }
            else
            {
                var maxId = 1;
                if (_context.Settings.Any())
                {
                    maxId = _context.Settings.Max(s => s.Id) + 1;
                }
                nextRunDateTimeSettings = new DbClient.Model.Settings()
                {
                    Id = maxId,
                    ParamName = _nextRunDateTimeField,
                    ParamValue = _nextRunDateTime.ToString()
                };
                _context.Settings.Add(nextRunDateTimeSettings);
                _context.SaveChanges();
            }
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
