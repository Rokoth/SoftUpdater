//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SoftUpdater.Common;
using SoftUpdater.Db.Context;
using SoftUpdater.Db.Interface;
using SoftUpdater.Db.Repository;
using SoftUpdater.Deploy;
using SoftUpdater.Service;

namespace SoftUpdater.SoftUpdaterHost
{
    /// <summary>
    /// Initiate app
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CommonOptions>(Configuration);
            services.AddControllersWithViews();
            services.AddLogging();
            services.AddDbContextPool<DbPgContext>((opt) =>
            {
                opt.EnableSensitiveDataLogging();
                var connectionString = Configuration.GetConnectionString("MainConnection");
                opt.UseNpgsql(connectionString);
            });
            
            services.AddCors();
            services.AddAuthentication()
            .AddJwtBearer("Token", options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //// укзывает, будет ли валидироваться издатель при валидации токена
                    ValidateIssuer = true,
                    //// строка, представляющая издателя
                    ValidIssuer = AuthOptions.ISSUER,

                    //// будет ли валидироваться потребитель токена
                    ValidateAudience = true,
                    //// установка потребителя токена
                    ValidAudience = AuthOptions.AUDIENCE,
                    //// будет ли валидироваться время существования
                    ValidateLifetime = true,

                    // установка ключа безопасности
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    // валидация ключа безопасности
                    ValidateIssuerSigningKey = true,

                };
            }).AddCookie("Cookies", options => {
                options.LoginPath = new PathString("/Account/Login");
                options.LogoutPath = new PathString("/Account/Logout");                
            });

            services
                .AddAuthorization(options =>
                {
                    var cookiePolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes("Cookies")
                        .Build();
                    options.AddPolicy("Cookie", cookiePolicy);
                    options.AddPolicy("Token", new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes("Token")
                        .Build());
                    options.DefaultPolicy = cookiePolicy;
                });

            services.AddScoped<IRepository<Db.Model.User>, Repository<Db.Model.User>>();
            services.AddScoped<IRepository<Db.Model.Client>, Repository<Db.Model.Client>>();
            services.AddScoped<IRepository<Db.Model.Release>, Repository<Db.Model.Release>>();
            services.AddScoped<IRepository<Db.Model.ReleaseArchitect>, Repository<Db.Model.ReleaseArchitect>>();
            services.AddScoped<IRepository<Db.Model.UserHistory>, Repository<Db.Model.UserHistory>>();
            services.AddScoped<IRepository<Db.Model.ClientHistory>, Repository<Db.Model.ClientHistory>>();
            services.AddScoped<IRepository<Db.Model.ReleaseHistory>, Repository<Db.Model.ReleaseHistory>>();
            services.AddScoped<IRepository<Db.Model.ReleaseArchitectHistory>, Repository<Db.Model.ReleaseArchitectHistory>>();
            services.AddDataServices();
            services.AddScoped<IDeployService, DeployService>();
            //services.AddScoped<INotifyService, NotifyService>();
            services.ConfigureAutoMapper();
            services.AddSwaggerGen();
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
