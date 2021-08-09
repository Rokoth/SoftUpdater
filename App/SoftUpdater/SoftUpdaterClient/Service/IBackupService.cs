using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public interface IBackupService
    {
        Task<bool> Backup(string appDir, string backupDir, string[] connectionStrings, List<string> ignoreDirectories, List<string> ignoreFiles);
    }

    public interface IRollBackService
    {
        Task<bool> RollBack(string appDir, string backupDir, string[] connectionStrings, List<string> ignoreDirectories, List<string> ignoreFiles);
    }
}