using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftUpdater.Common;
using System;

namespace SoftUpdaterClient.Service
{
    public class CheckUpdatesHostedService : CheckUpdatesHostedServiceBase
    {
        public CheckUpdatesHostedService(IServiceProvider serviceProvider) : base(serviceProvider, _serviceProvider => {
            var options = serviceProvider.GetRequiredService<IOptions<ClientOptions>>().Value;
            return new UpdateOptions()
            {
                Architecture = options.Architecture,
                CheckUpdateSchedule = options.CheckUpdateSchedule,
                Login = options.Login,
                Password = options.Password,
                ReleasePath = options.ReleasePath
            };
        }, "NextRunDateTime", "DownloadedVersion")
        {

        }
    }
}
