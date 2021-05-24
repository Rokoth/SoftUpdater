using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public class ReleaseHistoryDataService : DataGetService<Db.Model.ReleaseHistory, Contract.Model.ReleaseHistory,
        Contract.Model.ReleaseHistoryFilter>
    {
        public ReleaseHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override async Task<Contract.Model.ReleaseHistory> Enrich(Contract.Model.ReleaseHistory release, CancellationToken token)
        {
            var clientDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.Client, Contract.Model.ClientFilter>>();
            release.Client = await clientDataService.GetAsync(release.ClientId, token);
            return release;
        }

        protected override async Task<IEnumerable<Contract.Model.ReleaseHistory>> Enrich(IEnumerable<Contract.Model.ReleaseHistory> releases, CancellationToken token)
        {
            var clientDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.Client, Contract.Model.ClientFilter>>();
            foreach (var release in releases)
            {
                release.Client = await clientDataService.GetAsync(release.ClientId, token);
            }
            return releases;
        }

        protected override string DefaultSort => "ChangeDate desc";

        protected override Expression<Func<Db.Model.ReleaseHistory, bool>> GetFilter(Contract.Model.ReleaseHistoryFilter filter)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name))
                && (filter.Id == null || s.Id == filter.Id);
        }
    }
}
