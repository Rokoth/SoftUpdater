using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public interface IAddDataService<Tdto, TCreator> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> AddAsync(TCreator entity, CancellationToken token);
    }
}
