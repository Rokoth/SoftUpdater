using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public interface IAuthService
    {
        Task<ClaimsIdentity> Auth(Contract.Model.ClientIdentity login, CancellationToken token);
        Task<ClaimsIdentity> Auth(Contract.Model.UserIdentity login, CancellationToken token);
    }
}
