using System;
using System.Linq.Expressions;

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
            //return s => filter.Name == null || s.Name.Contains(filter.Name);
            return s => true;
        }

        protected override Db.Model.Release UpdateFillFields(Contract.Model.ReleaseUpdater entity, Db.Model.Release entry)
        {
            return entry;
        }
       
        protected override string DefaultSort => "Name";

    }
}
