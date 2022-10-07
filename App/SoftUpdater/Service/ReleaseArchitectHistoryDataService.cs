using System;
using System.Linq.Expressions;

namespace SoftUpdater.Service
{
    public class ReleaseArchitectHistoryDataService : DataGetService<Db.Model.ReleaseArchitectHistory, Contract.Model.ReleaseArchitectHistory,
        Contract.Model.ReleaseArchitectHistoryFilter>
    {
        public ReleaseArchitectHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override string DefaultSort => "ChangeDate desc";

        protected override Expression<Func<Db.Model.ReleaseArchitectHistory, bool>> GetFilter(Contract.Model.ReleaseArchitectHistoryFilter filter)
        {
            return s => (filter.Id == null || s.Id == filter.Id);
        }
    }
}
