using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftUpdater.Common;
using System;
using System.Collections.Generic;

namespace SoftUpdaterClient.Service
{
    public class CheckSelfUpdatesHostedService : CheckUpdatesHostedServiceBase
    {
        public CheckSelfUpdatesHostedService(IServiceProvider serviceProvider) : base(serviceProvider, _serviceProvider=> {
            var options = serviceProvider.GetRequiredService<IOptions<ClientOptions>>().Value;
            return new UpdateOptions() { 
               Architecture = options.ArchitectureSelf,
               CheckUpdateSchedule = options.CheckUpdateScheduleSelf,               
               Login = options.LoginSelf,
               Password = options.PasswordSelf,
               ReleasePath = options.ReleasePathSelf
            };
        }, "NextRunDateTimeSelf", "DownloadedVersionSelf")
        { 
        
        }
    }
}
