using AutoMapper;
using DbClient.Context;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using SoftUpdater.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Topshelf;

namespace SoftUpdater.SoftUpdaterClient
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
                x.SetDescription($"SoftUpdater Service, 2021 (ñ)");
                x.SetDisplayName($"SoftUpdater Service");
                x.SetServiceName($"SoftUpdaterService");
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
                })
                .UseKestrel();

            return builder;
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<TreeCreator, Tree>()
            //    .ForMember(s => s.Id, s => s.MapFrom(c => Helper.GenerateGuid(new string[] { c.Name })))
            //    .ForMember(s => s.VersionDate, s => s.MapFrom(c => DateTimeOffset.Now));

            //CreateMap<TreeUpdater, Tree>()
            //    .ForMember(s => s.Id, s => s.MapFrom(c => Helper.GenerateGuid(new string[] { c.Name })))
            //    .ForMember(s => s.VersionDate, s => s.MapFrom(c => DateTimeOffset.Now));

            //CreateMap<Db.Model.User, Contract.Model.User>();

            //CreateMap<TreeItem, TreeItemModel>();

            //CreateMap<FormulaCreator, Formula>();

            //CreateMap<FormulaUpdater, Formula>();

            //CreateMap<Formula, FormulaModel>();
            //CreateMap<TreeHistory, TreeHistoryModel>();
            //CreateMap<TreeItemHistory, TreeItemHistoryModel>();
            //CreateMap<FormulaHistory, FormulaHistoryModel>();
        }
    }

    public static class CustomExtensionMethods
    {
        public static IConfigurationBuilder AddDbConfiguration(this IConfigurationBuilder builder)
        {
            var configuration = builder.Build();
            var mode = configuration.GetValue<RunMode>("Mode");
            if (mode == RunMode.Normal)
            {
                var connectionString = configuration.GetConnectionString("MainConnection");
                builder.AddConfigDbProvider(options => options.UseSqlite(connectionString), connectionString);
            }
            return builder;
        }

        public static IConfigurationBuilder AddConfigDbProvider(
            this IConfigurationBuilder configuration, Action<DbContextOptionsBuilder> setup, string connectionString)
        {
            configuration.Add(new ConfigDbSource(setup, connectionString));
            return configuration;
        }

        public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services)
        {
            var mappingConfig = new AutoMapper.MapperConfiguration(mc => mc.AddProfile(new MappingProfile()));

            var mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
            return services;
        }
    }

    public class ConfigDbSource : IConfigurationSource
    {
        private readonly Action<DbContextOptionsBuilder> _optionsAction;
        private string _connectionString;

        public ConfigDbSource(Action<DbContextOptionsBuilder> optionsAction, string connectionString)
        {
            _optionsAction = optionsAction;
            _connectionString = connectionString;
        }

        public Microsoft.Extensions.Configuration.IConfigurationProvider Build(IConfigurationBuilder builder)
        {            
            return new ConfigDbProvider(_optionsAction);
        }
    }

    public class ConfigDbProvider : ConfigurationProvider
    {
        private readonly Action<DbContextOptionsBuilder> _options;
       

        public ConfigDbProvider(Action<DbContextOptionsBuilder> options)
        {
            _options = options;            
        }

        public override void Load()
        {
            LoadInternal();
        }

        private void LoadInternal()
        {
            var builder = new DbContextOptionsBuilder<DbSqLiteContext>();
            _options(builder);

            using (var context = new DbSqLiteContext(builder.Options))
            {
                var items = context.Settings
                    .AsNoTracking()
                    .ToList();

                foreach (var item in items)
                {
                    Data.Add(item.ParamName, item.ParamValue);
                }
            }
        }
    }
}
