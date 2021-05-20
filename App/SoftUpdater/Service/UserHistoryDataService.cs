using System;
using System.Linq.Expressions;

namespace SoftUpdater.Service
{
    public class UserHistoryDataService : DataGetService<Db.Model.UserHistory, Contract.Model.UserHistory,
        Contract.Model.UserHistoryFilter>
    {
        public UserHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override string DefaultSort => "Name";

        protected override Expression<Func<Db.Model.UserHistory, bool>> GetFilter(Contract.Model.UserHistoryFilter filter)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name)) 
                && (filter.Id == null || s.Id == filter.Id);
        }
    }
}
