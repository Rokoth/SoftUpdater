using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public class ReleaseDataService : DataService<Db.Model.Release, Contract.Model.Release,
        Contract.Model.ReleaseFilter, Contract.Model.ReleaseCreator, Contract.Model.ReleaseUpdater>
    {
        public ReleaseDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.Release, bool>> GetFilter(Contract.Model.ReleaseFilter filter)
        {
            return s => filter.Clients.Contains(s.ClientId);            
        }

        protected override Db.Model.Release UpdateFillFields(Contract.Model.ReleaseUpdater entity, Db.Model.Release entry)
        {
            return entry;
        }
       
        protected override string DefaultSort => "Version";

        protected override async Task<Db.Model.Release> MapToEntityAdd(Contract.Model.ReleaseCreator creator)
        {
            var result = await base.MapToEntityAdd(creator);
            var _repo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.Release>>();
            var lastRelease = await _repo.GetAsync(new Db.Model.Filter<Db.Model.Release>() { 
               Page = 0, Selector = s=>s.ClientId == creator.ClientId, Size = 1, Sort = "Number desc"
            }, CancellationToken.None);
            result.Number = lastRelease.Data.FirstOrDefault()?.Number ?? 1;
            return result;
        }
    }

    public class ReleaseArchitectDataService : DataService<Db.Model.ReleaseArchitect, Contract.Model.ReleaseArchitect,
        Contract.Model.ReleaseArchitectFilter, Contract.Model.ReleaseArchitectCreator, Contract.Model.ReleaseArchitectUpdater>
    {
        public ReleaseArchitectDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.ReleaseArchitect, bool>> GetFilter(Contract.Model.ReleaseArchitectFilter filter)
        {
            return s => s.ReleaseId == filter.ReleaseId && (string.IsNullOrEmpty(filter.Name) || s.Name == filter.Name);
        }

        protected override Db.Model.ReleaseArchitect UpdateFillFields(Contract.Model.ReleaseArchitectUpdater entity, Db.Model.ReleaseArchitect entry)
        {
            return entry;
        }

        protected override string DefaultSort => "Name";                
    }

    public class LoadHistoryDataService : DataService<Db.Model.LoadHistory, Contract.Model.LoadHistory,
        Contract.Model.LoadHistoryFilter, Contract.Model.LoadHistoryCreator, Contract.Model.LoadHistoryUpdater>
    {
        public LoadHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.LoadHistory, bool>> GetFilter(Contract.Model.LoadHistoryFilter filter)
        {
            return s => true;
        }

        protected override Db.Model.LoadHistory UpdateFillFields(Contract.Model.LoadHistoryUpdater entity, Db.Model.LoadHistory entry)
        {
            throw new NotImplementedException();
        }

        protected override string DefaultSort => "Version";

        protected override async Task<Db.Model.LoadHistory> MapToEntityAdd(Contract.Model.LoadHistoryCreator creator)
        {
            var result = await base.MapToEntityAdd(creator);            
            return result;
        }
    }
}
