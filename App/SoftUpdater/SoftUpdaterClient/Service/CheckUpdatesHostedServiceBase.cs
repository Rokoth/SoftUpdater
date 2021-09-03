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
        private DateTimeOffset? _nextRunDateTime;
        private readonly CronExpression _expression;
        private UpdateOptions _options;       

        public CheckUpdatesHostedServiceBase(IServiceProvider serviceProvider, Func<IServiceProvider, UpdateOptions> configureOptions, 
            string nextRunDateTimeField, string downloadedVersionField)
        {
            _serviceProvider = serviceProvider;
            _nextRunDateTimeField = nextRunDateTimeField;
            _downloadedVersionField = downloadedVersionField;
           
            _logger = _serviceProvider.GetRequiredService<ILogger<CheckUpdatesHostedServiceBase>>();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            
            _options = configureOptions(_serviceProvider);
            if (string.IsNullOrEmpty(_options.CheckUpdateSchedule)) throw new ArgumentNullException("Options::CheckUpdateSchedule");
            _expression = CronExpression.Parse(_options.CheckUpdateSchedule, CronFormat.IncludeSeconds);                        
        }

        private async Task Run()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var _context = provider.GetRequiredService<DbSqLiteContext>();
                var _httpClient = provider.GetRequiredService<IClientHttpClient>();
                GetNextRunDateTime(_context);
                while (!_cancellationToken.IsCancellationRequested)
                {
                    if (_nextRunDateTime <= DateTimeOffset.Now)
                    {
                        try
                        {
                            var isAuth = await _httpClient.Auth(new ClientIdentity()
                            {
                                Login = _options.Login,
                                Password = _options.Password
                            });
                            if (isAuth)
                            {
                                var downloadedVersion = _context.Settings.FirstOrDefault(s => s.ParamName == _downloadedVersionField);
                                ReleaseClient release = await _httpClient.GetLastRelease(downloadedVersion?.ParamValue);
                                if (release != null)
                                {
                                    var architect = release.Architects.FirstOrDefault(s => s.Name == _options.Architecture);
                                    if (architect != null)
                                    {
                                        using (Stream stream = await _httpClient.DownloadRelease(architect.Id))
                                        {
                                            if (!Directory.Exists(_options.ReleasePath))
                                            {
                                                Directory.CreateDirectory(_options.ReleasePath);
                                            }
                                            var releasePath = Path.Combine(_options.ReleasePath, release.Version);
                                            if (!Directory.Exists(releasePath))
                                            {
                                                Directory.CreateDirectory(releasePath);
                                            }
                                            using (FileStream tmp = new FileStream(Path.Combine(releasePath, architect.Name), FileMode.Create))
                                            {
                                                stream.CopyTo(tmp);
                                                tmp.Flush();
                                            }
                                        }
                                    }
                                    SaveSettings(_context, release.Version, _downloadedVersionField);
                                }                              
                            }
                            else
                                throw new Exception("Failed to Auth");                            
                        }
                        catch (Exception ex)
                        {
                            await _httpClient.SendErrorMessage($"Ошибка в CheckUpdatesHostedService: {ex.Message} {ex.StackTrace}");
                            _logger.LogError($"Ошибка в CheckUpdatesHostedService: {ex.Message} {ex.StackTrace}");
                        }
                        finally
                        {
                            GetNextRunDateTime(_context);
                        }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(1));                    
                }
            }
        }

        private void GetNextRunDateTime(DbSqLiteContext _context)
        {
            var nextRunDateTimeSettings = _context.Settings.FirstOrDefault(s => s.ParamName == _nextRunDateTimeField);
            if (_nextRunDateTime == null)
            {
                if (nextRunDateTimeSettings != null)
                {
                    if (DateTimeOffset.TryParse(nextRunDateTimeSettings.ParamValue, out DateTimeOffset nextRunDateTime))
                    {
                        _nextRunDateTime = nextRunDateTime;
                        return;
                    }
                }
                _nextRunDateTime = DateTimeOffset.Now;
            }
            _nextRunDateTime = _expression.GetNextOccurrence(_nextRunDateTime.Value, TimeZoneInfo.Local).Value;
            SaveSettings(_context, _nextRunDateTime.ToString(), _nextRunDateTimeField);
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
