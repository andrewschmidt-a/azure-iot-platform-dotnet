
using System;
using IdentityGateway.Services;
using IdentityGateway.Services.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityGateway.WebService
{
    public class Startup
    {
        private const string CORS_CONFIG_KEY = "Global:ClientAuth:CORSWhiteList";
        private const string CORS_POLICY_NAME = "identityCORS";
        // Initialized in `Startup`
        public IConfigurationRoot Configuration { get; }

        // Invoked by `Program.cs`
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
#if DEBUG
                .AddIniFile("appsettings.ini", optional: false, reloadOnChange: true)
#endif
                ;
            this.Configuration = builder.Build();
            builder.AddEnvironmentVariables();
            var settings = builder.Build();
            builder.AddAzureAppConfiguration(settings["PCS_APPLICATION_CONFIGURATION"]); 
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add CORS if available
            if (!String.IsNullOrEmpty(Configuration[CORS_CONFIG_KEY]))
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(CORS_POLICY_NAME,
                        builder =>
                        {
                            builder.WithOrigins(Configuration[CORS_CONFIG_KEY]);
                            //builder.AllowAnyOrigin();
                            builder.AllowCredentials();
                            builder.AllowAnyMethod();
                            builder.AllowAnyHeader();

                        });
                });
            }
            
            services.AddMvc().AddControllersAsServices();

            services.AddScoped<TableHelper>();
            
            services.AddSingleton<UserSettingsContainer>();
            services.AddSingleton<UserTenantContainer>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IStatusService, StatusService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Add CORS if available
            if (!String.IsNullOrEmpty(Configuration[CORS_CONFIG_KEY]))
            {
                app.UseCors(CORS_POLICY_NAME); 
            }
            app.UseMiddleware<AuthMiddleware>();
            app.UseMvc();
        }
    }
}
