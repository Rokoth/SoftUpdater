using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using SoftUpdater.Common;
using SoftUpdater.SoftUpdaterHost;
using SoftUpdaterClient.Service;

namespace TaskCollector.UnitTests
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

            serviceCollection.AddLogging(configure => configure.AddSerilog());                      

            serviceCollection.ConfigureAutoMapper();
            ServiceProvider = serviceCollection.BuildServiceProvider();        
        }

        
    }
}
