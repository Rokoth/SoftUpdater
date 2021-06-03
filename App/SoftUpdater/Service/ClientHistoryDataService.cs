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
            var user = await userDataService.GetAsync(client.UserId, token);
            if(user!=null) client.UserName = user.Name;
            return client;
        }

        protected override async Task<IEnumerable<Contract.Model.ClientHistory>> Enrich(IEnumerable<Contract.Model.ClientHistory> clients, CancellationToken token)
        {
            List<Contract.Model.ClientHistory> result = new List<Contract.Model.ClientHistory>();
            var userDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.User, Contract.Model.UserFilter>>();
            foreach (var client in clients)
            {
                var user = await userDataService.GetAsync(client.UserId, token);
                if (user != null) client.UserName = (await userDataService.GetAsync(client.UserId, token)).Name;
                result.Add(client);
            }
            return result;
        }

        protected override string DefaultSort => "Name";

        protected override Expression<Func<Db.Model.ClientHistory, bool>> GetFilter(Contract.Model.ClientHistoryFilter filter)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name))
                && (filter.Id == null || s.Id == filter.Id);
        }

        protected override Func<Db.Model.Filter<Db.Model.ClientHistory>, CancellationToken,
            Task<Contract.Model.PagedResult<Db.Model.ClientHistory>>> GetListFunc(Db.Interface.IRepository<Db.Model.ClientHistory> repo)
        {
            return repo.GetAsyncDeleted;
        }
    }
}
