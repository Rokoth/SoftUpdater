using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Topshelf;

namespace SoftUpdaterDebugAndTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var loggerConfig = new LoggerConfiguration()
               .WriteTo.Console()
               .WriteTo.File("Logs\\log-startup.txt")
               .MinimumLevel.Verbose();

            using var logger = loggerConfig.CreateLogger();
            logger.Information($"Service starts with arguments: {string.Join(", ", args)}");

            var exitCode = HostFactory.Run(x =>
            {
                x.Service<Starter>(s =>
                {
                    s.ConstructUsing(_ => new Starter(logger, args));
                    s.WhenStarted(starter => starter.Start());
                    s.WhenStopped(starter => starter.Stop());
                });

                x.RunAsLocalService();
                x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(10)));
                x.SetDescription($"SoftUpdaterDebug Service, 2021 (ñ)");
                x.SetDisplayName($"SoftUpdaterDebug Service");
                x.SetServiceName($"SoftUpdaterDebugService");
                x.StartAutomatically();
            });
            logger.Information($"Service stops with exit code: {exitCode}");
        }
    }

    public class Starter
    {
        private IWebHost webHost;
        private readonly string[] Args;
        private ILogger _logger;

        public Starter(ILogger logger, string[] args)
        {
            _logger = logger;
            Args = args;
        }

        public bool Start()
        {
            try
            {
                webHost = BuildWebHost(GetWebHostBuilder(Args));
                _logger.Information("Start service...");
                webHost.Start();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Starting service error! \nException:\n {ex.Message} \nStackTrace:\n {ex.StackTrace} ");
                throw;
            }
        }

        public bool Stop()
        {
            webHost?.StopAsync().ContinueWith(s =>
            {
                if (s.IsFaulted)
                {
                    _logger.Error($"Stopping service error! \nException:\n {s.Exception}");
                }
                webHost?.Dispose();
            });
            return true;
        }

        private IWebHost BuildWebHost(IWebHostBuilder webHostBuilder)
        {
            return webHostBuilder.UseStartup<Startup>()
                .Build();
        }

        protected IWebHostBuilder GetWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(
                    new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()                  
                    .Build())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .CreateLogger();
                    logging.AddSerilog(Log.Logger);
                })
                .UseKestrel();

            return builder;
        }
    }
}
