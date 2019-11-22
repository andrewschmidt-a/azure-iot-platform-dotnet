
using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Mmm.Platform.IoT.IdentityGateway.Services;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Mmm.Platform.IoT.Common.AppConfiguration;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.WebService.Auth;
using Mmm.Platform.IoT.Common.WebService.Runtime;

namespace Mmm.Platform.IoT.IdentityGateway.WebService
{
    public class Startup
    {
        private const string CORS_CONFIG_KEY = "Global:ClientAuth:CORSWhiteList";
        private const string CORS_POLICY_NAME = "identityCORS";
        private const string APP_CONFIGURATION = "PCS_APPLICATION_CONFIGURATION";

        private readonly List<string> appConfigKeys = new List<string>
        {
            "Global",
            "Global:KeyVault",
            "Global:AzureActiveDirectory",
            "Global:ClientAuth"
        };

        // Initialized in `Startup`
        public IConfigurationRoot Configuration { get; }
        // Initialized in `ConfigureServices`
        public IContainer ApplicationContainer { get; private set; }

        // Invoked by `Program.cs`
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
#if DEBUG
                .AddIniFile("appsettings.ini", optional: true, reloadOnChange: true)
#endif
                .AddEnvironmentVariables();
            // build configuration with environment variables
            var preConfig = builder.Build();
            // Add app config settings to the configuration builder
            builder.Add(new AppConfigurationSource(preConfig[APP_CONFIGURATION], this.appConfigKeys));
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc($"v1", new OpenApiInfo { Title = "Identity Gateway API", Version = "v1" });
            });

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();

            services.AddScoped<TableHelper>();
            services.AddSingleton<UserSettingsContainer>();
            services.AddSingleton<UserTenantContainer>();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IStatusService, StatusService>();
            services.AddTransient<IOpenIdProviderConfiguration, OpenIdProviderConfiguration>();
            services.AddSingleton<IRsaHelpers, RsaHelpers>();
            services.AddTransient<ISendGridClientFactory, SendGridClientFactory>();
            services.AddTransient<ITableHelper, TableHelper>();

            // Prepare DI container
            this.ApplicationContainer = new DependencyResolution().Setup(services);

            // Print some useful information at bootstrap time
            this.PrintBootstrapInfo(this.ApplicationContainer);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("./swagger/v1/swagger.json", "V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseMiddleware<AuthMiddleware>();
            app.UseMvc();
        }
        private void PrintBootstrapInfo(IContainer container)
        {
            var log = container.Resolve<ILogger>();
            log.Info("Web service started", () => new { Uptime.ProcessId });
        }
    }
}
