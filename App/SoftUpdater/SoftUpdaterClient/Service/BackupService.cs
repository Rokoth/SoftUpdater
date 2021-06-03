using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public class BackupService : IBackupService
    {
        private IServiceProvider _serviceProvider;
        private ILogger _logger;
        private IServiceHelper _serviceHelper;

        public BackupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<BackupService>>();
            _serviceHelper = _serviceProvider.GetRequiredService<IServiceHelper>();
        }

        public async Task<bool> Backup(string appDir, string backupDir, string[] connectionStrings)
        {
            try
            {
                var s = "Server=localhost;Database=soft_updater;Username=postgres;Password=Rok_Oth_123";
                var dumped = true;
                foreach (var connectionString in connectionStrings)
                {
                    var parsed = _serviceHelper.Parse(connectionString);
                    var host = parsed["Server"];
                    var port = parsed.ContainsKey("Port") ? parsed["Port"] : "5432";
                    var database = parsed["Database"];
                    var user = parsed["Username"];
                    var pass = parsed["Password"];
                    dumped |= await _serviceHelper.PostgreSqlDump(Path.Combine(backupDir, $"backup_{DateTime.Now:yyyyMMddhhmmss}.backup"), 
                        host, port, database, user, pass);
                }

                if (!dumped) return false;
                return _serviceHelper.DirectoryCopy(appDir, backupDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при резервном копировании: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }
    }
}
