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
    public class SelfUpdateHostedService : IHostedService
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

        public SelfUpdateHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = new CancellationTokenSource();
            _context = _serviceProvider.GetRequiredService<DbSqLiteContext>();
            _options = _serviceProvider.GetRequiredService<IOptions<ClientOptions>>().Value;
            _downloadedVersionField = _options.DownloadedSelfVersionField;
            _installedVersionField = _options.InstalledSelfVersionField;
            _nextRunDateTimeField = _options.NextRunDateTimeSelfField;
            _expression = CronExpression.Parse(_options.InstallSelfSchedule);
            _logger = _serviceProvider.GetRequiredService<ILogger<SelfUpdateHostedService>>();
            GetNextRunDateTime();
            httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
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
                            var installService = _serviceProvider.GetRequiredService<IInstallSelfService>();
                            if (installService.Install(new InstallSettings()
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
                                TmpDir = _options.ReleasePath
                            }))
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
                    if (downLoaded[i] > installed[i]) return true;
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
                _context.Settings.Add(new DbClient.Model.Settings()
                {
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
