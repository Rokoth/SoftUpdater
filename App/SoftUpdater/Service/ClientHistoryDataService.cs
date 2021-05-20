using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public class ClientHistoryDataService : DataGetService<Db.Model.ClientHistory, Contract.Model.ClientHistory,
        Contract.Model.ClientHistoryFilter>
    {        
        public ClientHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }

        protected override async Task<Contract.Model.ClientHistory> Enrich(Contract.Model.ClientHistory client, CancellationToken token)
        {
            var userDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.User, Contract.Model.UserFilter>>();
            client.User = await userDataService.GetAsync(client.UserId, token);
            return client;
        }

        protected override async Task<IEnumerable<Contract.Model.ClientHistory>> Enrich(IEnumerable<Contract.Model.ClientHistory> clients, CancellationToken token)
        {
            var userDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.User, Contract.Model.UserFilter>>();
            foreach (var client in clients)
            {
                client.User = await userDataService.GetAsync(client.UserId, token);
            }
            return clients;
        }

        protected override string DefaultSort => "Name";

        protected override Expression<Func<Db.Model.ClientHistory, bool>> GetFilter(Contract.Model.ClientHistoryFilter filter)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name))
                && (filter.Id == null || s.Id == filter.Id);
        }
    }
}
