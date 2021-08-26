using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public interface ISelfUpdateService
    {
        Task Execute();
    }
}