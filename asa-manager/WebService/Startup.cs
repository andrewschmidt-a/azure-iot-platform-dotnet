using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Mmm.Platform.IoT.Common.Services.Auth;

namespace Mmm.Platform.IoT.AsaManager.WebService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc($"v1", new OpenApiInfo { Title = "ASA Manager API", Version = "v1" });
            });

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();
            this.ApplicationContainer = new DependencyResolution().Setup(services, Configuration);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        private void LogDependencyInjectionContainerRegistrations(ILogger logger)
        {
            foreach (var registration in ApplicationContainer.ComponentRegistry.Registrations)
            {
                logger.LogTrace("Type {type} is registered in dependency injection container", registration.Activator.ToString());
            }
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            LogDependencyInjectionContainerRegistrations(logger);

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("./swagger/v1/swagger.json", "V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseMiddleware<ClientToClientAuthMiddleware>();
            app.UseMvc();

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
        }
    }
}
