// Copyright (c) Microsoft. All rights reserved.

using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Mmm.Platform.IoT.Common.WebService.Auth;
using Mmm.Platform.IoT.Common.WebService.Runtime;
using ILogger = Mmm.Platform.IoT.Common.Services.Diagnostics.ILogger;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService
{
    public class Startup
    {
        // Initialized in `Startup`
        public IConfigurationRoot Configuration { get; }

        // Initialized in `ConfigureServices`
        public IContainer ApplicationContainer { get; private set; }

        // Invoked by `Program.cs`
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            this.Configuration = builder.Build();
        }

        // This is where you register dependencies, add services to the
        // container. This method is called by the runtime, before the
        // Configure method below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc($"v1", new OpenApiInfo { Title = "Config API", Version = "v1" });
            });

            // Setup (not enabling yet) CORS 
            services.AddCors();
            //services.AddIoTTokenValidator(new IoTTokenValidatorOptions
            //{
            //    Authority = "https://aziotidentity3m.centralus.cloudapp.azure.com"
            //});

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.Authority = "https://aziotidentity3m.centralus.cloudapp.azure.com";
            //        options.Audience = "IoTPlatform";
            //        options.RequireHttpsMetadata = false;
            //        options.EventsType = typeof(CustomJwtBearerEvents);
            //    });

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();

            // Prepare DI container
            this.ApplicationContainer = new DependencyResolution().Setup(services);

            // Print some useful information at bootstrap time
            this.PrintBootstrapInfo(this.ApplicationContainer);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method is called by the runtime, after the ConfigureServices
        // method above. Use this method to add middleware.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ICorsSetup corsSetup,
            IApplicationLifetime appLifetime)
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

            // Check for Authorization header before dispatching requests
            app.UseMiddleware<AuthMiddleware>();

            // Enable CORS - Must be before UseMvc
            // see: https://docs.microsoft.com/en-us/aspnet/core/security/cors
            corsSetup.UseMiddleware(app);
            //app.UseAuthentication();

            app.UseMvc();
        }

        private void PrintBootstrapInfo(IContainer container)
        {
            var log = container.Resolve<ILogger>();
            log.Info("Web service started", () => new { Uptime.ProcessId });
        }
    }
}
