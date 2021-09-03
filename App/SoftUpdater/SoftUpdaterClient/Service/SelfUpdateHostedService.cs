using Cronos;
using DbClient.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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
            _expression = CronExpression.Parse(_options.InstallSelfSchedule, CronFormat.IncludeSeconds);
            _logger = _serviceProvider.GetRequiredService<ILogger<SelfUpdateHostedService>>();
            
            httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
        }

        private async Task Run()
        {
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
                            var _serviceHelper = _serviceProvider.GetRequiredService<IServiceHelper>();
                            _serviceHelper.DirectoryCopy(
                                Directory.GetCurrentDirectory(),
                                _options.SelfUpdateTempDir, 
                                new List<string>(), 
                                new List<string>(), 
                                true);

                            string appsettings = "";

                            using (var file = new StreamReader(Path.Combine(_options.SelfUpdateTempDir, "appsettings.json")))
                            {
                                appsettings = file.ReadToEnd();
                            }
                            JObject obj = JObject.Parse(appsettings);
                            AddOrReplaceToken(ref obj, "ApplicationSelfDirectory", Directory.GetCurrentDirectory());
                            AddOrReplaceToken(ref obj, "Mode", "SelfUpdate");

                            if (!Path.IsPathRooted(_options.BackupSelfDirectory))
                                AddOrReplaceToken(ref obj, "BackupSelfDirectory", Path.Combine(Directory.GetCurrentDirectory(), _options.BackupSelfDirectory));

                            if (!Path.IsPathRooted(_options.ReleasePathSelf))
                                AddOrReplaceToken(ref obj, "ReleasePathSelf", Path.Combine(Directory.GetCurrentDirectory(), _options.ReleasePathSelf));

                            using (var file = new StreamWriter(Path.Combine(_options.SelfUpdateTempDir, "appsettings.json")))
                            {
                                file.Write(obj.ToString());
                            }
                            await _serviceHelper.Execute($"{Path.Combine(_options.SelfUpdateTempDir, "SoftUpdater.exe")}");
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

        private void AddOrReplaceToken(ref JObject obj, string key, string value)
        {
            if (obj.ContainsKey(key))
            {                
                obj[key] = value;
            }
            else
            {
                obj.Add(key, value);
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
