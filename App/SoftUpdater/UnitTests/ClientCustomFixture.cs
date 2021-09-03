using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using SoftUpdater.Common;
using SoftUpdater.SoftUpdaterHost;
using SoftUpdaterClient.Service;
using SoftUpdater.ClientHttpClient;
using System;
using System.Threading.Tasks;
using SoftUpdater.Contract.Model;

namespace SoftUpdater.UnitTests
{
    public class ClientCustomFixture
    {        
        public ServiceProvider ServiceProvider { get; private set; }

        public ClientCustomFixture()
        {

            Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Verbose()
             .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "test-log.txt"))
             .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
                       
            serviceCollection.Configure<CommonOptions>(config);
            serviceCollection.AddScoped<IBackupService, BackupService>();
            serviceCollection.AddScoped<IServiceHelper, ServiceHelper>();
            serviceCollection.AddScoped<IClientHttpClient, HttpClientFixture>();
            serviceCollection.AddScoped<IUpdateScriptParser, UpdateScriptParser>();
            

            serviceCollection.AddLogging(configure => configure.AddSerilog());                      

            serviceCollection.ConfigureAutoMapper();
            ServiceProvider = serviceCollection.BuildServiceProvider();        
        }

        public class HttpClientFixture : IClientHttpClient
        {
            public bool IsConnected => true;

            public event EventHandler OnConnect;

            public async Task<bool> Auth(ClientIdentity identity)
            {
                await Task.CompletedTask;
                return true;
            }

            public void ConnectToServer(string server, Action<bool, bool, string> onResult)
            {
                
            }

            public void Dispose()
            {
                
            }

            public Task<Stream> DownloadRelease(Guid id)
            {
                throw new NotImplementedException();
            }

            public Task<ReleaseClient> GetLastRelease(string currentVersion)
            {
                throw new NotImplementedException();
            }

            public async Task<bool> SendErrorMessage(string message)
            {
                await Task.CompletedTask;
                return true;
            }
        }


    }
}
