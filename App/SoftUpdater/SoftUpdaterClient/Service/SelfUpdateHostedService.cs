using Cronos;
using DbClient.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
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

        public SelfUpdateHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                        //downloadedVersion.ParamValue
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        GetNextRunDateTime();
                    }
                }
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
