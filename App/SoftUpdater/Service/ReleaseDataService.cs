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
            return s => filter.ClientId == null || s.ClientId == filter.ClientId;            
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
}
