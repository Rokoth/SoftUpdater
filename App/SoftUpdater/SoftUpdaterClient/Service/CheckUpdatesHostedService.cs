using Cronos;
using DbClient.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUpdater.ClientHttpClient;
using SoftUpdater.Common;
using SoftUpdater.Contract.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
