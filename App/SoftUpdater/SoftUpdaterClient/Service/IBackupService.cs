using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public interface IBackupService
    {
        Task<bool> Backup(string appDir, string backupDir, string[] connectionStrings);
    }
}