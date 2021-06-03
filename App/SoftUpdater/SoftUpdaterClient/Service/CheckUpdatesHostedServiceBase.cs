using Cronos;
using DbClient.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftUpdater.ClientHttpClient;
using SoftUpdater.Contract.Model;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public abstract class CheckUpdatesHostedServiceBase : IHostedService
    {
        private string _nextRunDateTimeField;
        private string _downloadedVersionField ;        
        private IServiceProvider _serviceProvider;
        private ILogger<CheckUpdatesHostedServiceBase> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private DbSqLiteContext _context;
        private DateTimeOffset _nextRunDateTime;
        private readonly CronExpression _expression;
        private UpdateOptions _options;
        private IClientHttpClient _httpClient;

        public CheckUpdatesHostedServiceBase(IServiceProvider serviceProvider, Func<IServiceProvider, UpdateOptions> configureOptions, 
            string nextRunDateTimeField, string downloadedVersionField)
        {
            _serviceProvider = serviceProvider;
            _nextRunDateTimeField = nextRunDateTimeField;
            _downloadedVersionField = downloadedVersionField;
           
            _logger = _serviceProvider.GetRequiredService<ILogger<CheckUpdatesHostedServiceBase>>();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _context = _serviceProvider.GetRequiredService<DbSqLiteContext>();
            _options = configureOptions(_serviceProvider);
            if (string.IsNullOrEmpty(_options.CheckUpdateSchedule)) throw new ArgumentNullException("Options::CheckUpdateSchedule");
            _expression = CronExpression.Parse(_options.CheckUpdateSchedule);
            _httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
            GetNextRunDateTime();
        }

        private async Task Run()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (_nextRunDateTime <= DateTimeOffset.Now)
                {
                    try
                    {
                        if (await _httpClient.Auth(new ClientIdentity()
                        {
                            Login = _options.Login,
                            Password = _options.Password
                        }))
                        {
                            var downloadedVersion = _context.Settings.FirstOrDefault(s => s.ParamName == _downloadedVersionField);
                            ReleaseClient release = await _httpClient.GetLastRelease(downloadedVersion.ParamValue, _options.Architecture);
                            using (FileStream stream = await _httpClient.DownloadRelease(release.Architects.First().Id))
                            {
                                if (!Directory.Exists(_options.ReleasePath))
                                {
                                    Directory.CreateDirectory(_options.ReleasePath);
                                }
                                var releasePath = Path.Combine(_options.ReleasePath, release.Architects.First().Name);
                                if (!Directory.Exists(releasePath))
                                {
                                    Directory.CreateDirectory(releasePath);
                                }
                                using (FileStream tmp = new FileStream(Path.Combine(releasePath, stream.Name), FileMode.Create))
                                {
                                    stream.CopyTo(tmp);
                                    tmp.Flush();
                                }
                            }
                            
                            downloadedVersion.ParamValue = release.Version;
                            _context.Update(downloadedVersion);
                            _context.SaveChanges();
                        }
                        _nextRunDateTime = _expression.GetNextOccurrence(_nextRunDateTime, TimeZoneInfo.Local).Value;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Ошибка в CheckUpdatesHostedService: {ex.Message} {ex.StackTrace}");
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private void GetNextRunDateTime()
        {
            var nextRunDateTimeSettings = _context.Settings.FirstOrDefault(s => s.ParamName == _nextRunDateTimeField);
            if (nextRunDateTimeSettings != null)
            {
                if (DateTimeOffset.TryParse(nextRunDateTimeSettings.ParamValue, out _nextRunDateTime)) return;
            }
            _nextRunDateTime = _expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local).Value;
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
