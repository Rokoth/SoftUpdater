using DbClient.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SoftUpdater.ClientHttpClient;
using SoftUpdater.Common;
using SoftUpdater.Contract.Model;
using SoftUpdaterClient.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUpdater.SoftUpdaterClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ClientOptions>(Configuration);
            services.AddScoped<IClientHttpClient, ClientHttpClient.ClientHttpClient>();
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IInstallService, InstallService>();
            services.AddScoped<IRollBackService, RollBackService>();
            services.AddScoped<IServiceHelper, ServiceHelper>();
            services.AddScoped<ISelfUpdateService, SelfUpdateService>();
            services.AddScoped<IUpdateScriptParser, UpdateScriptParser>();
            services.AddScoped<IHttpClientSettings, HttpClientSettings>();

            services.AddDbContext<DbSqLiteContext>(options=> {
                options.UseSqlite(Configuration.GetConnectionString("LocalConnection"));
                options.EnableSensitiveDataLogging();
             });

            services.AddLogging();
            services.AddCors();                        
            services.ConfigureAutoMapper();

            services.AddHostedService<CheckUpdatesHostedService>();
            services.AddHostedService<InstallHostedService>();
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            var options = app.ApplicationServices.GetRequiredService<IOptions<ClientOptions>>().Value;
            if (options.Mode == RunMode.SelfUpdate)
            {
                try
                {
                    var service = app.ApplicationServices.GetRequiredService<ISelfUpdateService>();
                    service.Execute().GetAwaiter().GetResult();
                }
                catch (Exception)
                {

                }
                finally
                {
                    var _lifeTime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                    _lifeTime.StopApplication();
                }
            }
        }        
        
    }

    public class HttpClientSettings : IHttpClientSettings
    {
        public HttpClientSettings(IOptions<ClientOptions> options)
        {
            Server = options.Value.Server;
        }

        public Dictionary<Type, string> Apis => new Dictionary<Type, string>()
        {
            { typeof(Release), "api/v1/release"}, { typeof(ReleaseArchitect), "api/v1/release/download"}, { typeof(ClientIdentity), "api/v1/auth/auth"}
        };

        public string Server { get; }
    }
}
