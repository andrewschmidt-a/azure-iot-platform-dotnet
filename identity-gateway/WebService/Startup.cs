
using System.Linq;
using IdentityGateway.Services;
using IdentityGateway.Services.Runtime;
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
            // build configuration with environment variables
            var preConfig = builder.Build();
            // Add app config settings to the configuration builder
            this.SetUpAppConfigSettings(builder, preConfig["PCS_APPLICATION_CONFIGURATION"]);
            Configuration = builder.Build();
        }

        private void SetUpAppConfigSettings(IConfigurationBuilder builder, string appConfigConnectionString)
        {
            // Get all app config settings in a config root
            ConfigurationBuilder appConfigBuilder = new ConfigurationBuilder();
            appConfigBuilder.AddAzureAppConfiguration(appConfigConnectionString);
            IConfigurationRoot appConfig = appConfigBuilder.Build();

            // Settings and children added to this.configuration are chosen based on elements in this list
            var appConfigKeys = AppConfigSettings.AppConfigSettingKeys;

            // Add new configurations
            foreach (var key in appConfigKeys)
            {
                // keys are the section strings, values are the binding instances
                IConfiguration keySettings = appConfig.GetSection(key);
                var keySettingsResult = keySettings.GetChildren().ToList();
                foreach (var config in keySettingsResult)
                {
                    builder.AddConfiguration(config);
                }
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
            app.UseMiddleware<AuthMiddleware>();
            app.UseMvc();
        }
    }
}
