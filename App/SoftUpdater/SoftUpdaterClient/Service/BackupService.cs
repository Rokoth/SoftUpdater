using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.ClientHttpClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SoftUpdaterClient.Service
{
    public class BackupService : IBackupService
    {
        private IServiceProvider _serviceProvider;
        private ILogger _logger;
        private IServiceHelper _serviceHelper;
        private IClientHttpClient httpClient;

        public BackupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<BackupService>>();
            _serviceHelper = _serviceProvider.GetRequiredService<IServiceHelper>();
            httpClient = _serviceProvider.GetRequiredService<IClientHttpClient>();
        }

        public async Task<bool> Backup(string appDir, string backupDir, string[] connectionStrings, List<string> ignoreDirectories, List<string> ignoreFiles)
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
                    dumped |= await _serviceHelper.PostgreSqlDump(Path.Combine(backupDir, $"backup_{database}_{DateTime.Now:yyyyMMddhhmmss}.backup"), 
                        host, port, database, user, pass);
                }

                if (!dumped) return false;
                return _serviceHelper.DirectoryCopy(appDir, backupDir, ignoreDirectories, ignoreFiles,  true);
            }
            catch (Exception ex)
            {
                await httpClient.SendErrorMessage($"Ошибка при резервном копировании: {ex.Message} {ex.StackTrace}");
                _logger.LogError($"Ошибка при резервном копировании: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }
    }
}
