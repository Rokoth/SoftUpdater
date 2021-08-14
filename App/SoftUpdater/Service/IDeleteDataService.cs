using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public interface IDeleteDataService<Tdto> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> DeleteAsync(Guid id, CancellationToken token);
    }
}
