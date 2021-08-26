using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    internal interface ISelfBackupService
    {
        Task<bool> Backup(string applicationDirectory, string backupDirectory, string[] vs, List<string> lists1, List<string> lists2);
    }
}