using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public interface IServiceHelper
    {
        bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs);
        Task<bool> Execute(string command);
        bool ExecuteCommand(string command);
        Task<bool> PostgreSqlDump(string outFile, string host, string port, string database, string user, string password);
        Task<bool> PostgreSqlRestore(string inputFile, string host, string port, string database, string user, string password);
        Dictionary<string, string> Parse(string connectionString);
    }
}