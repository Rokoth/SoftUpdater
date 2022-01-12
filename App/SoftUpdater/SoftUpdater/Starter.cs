///Copyright 2021 Dmitriy Rokoth
///Licensed under the Apache License, Version 2.0
///
///ref 1

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore;
using Serilog;
using System.IO;
using SoftUpdater.Common;

namespace SoftUpdater.SoftUpdaterHost
{
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
                    .AddDbConfiguration()
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
                    logging.AddErrorNotifyLogger(config => {
                        config.Options = hostingContext.Configuration.GetValue<ErrorNotifyOptions>("ErrorNotifyOptions");
                    });
                })
                .UseKestrel();

            return builder;
        }
    }
}
