using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SoftUpdater.ClientHttpClient;
using SoftUpdater.Common;
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
            services.AddLogging();
            services.AddCors();                        
            services.ConfigureAutoMapper();            
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
                var service = app.ApplicationServices.GetRequiredService<ISelfUpdateService>();
                service.Execute().GetAwaiter().GetResult();
                var _lifeTime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                _lifeTime.StopApplication();
            }
        }

        
    }
}
