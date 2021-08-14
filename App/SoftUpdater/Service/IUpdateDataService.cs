using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public interface IUpdateDataService<Tdto, TUpdater> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> UpdateAsync(TUpdater entity, CancellationToken token);
    }
}
