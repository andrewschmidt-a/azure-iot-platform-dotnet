// <copyright file="Startup.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Mmm.Iot.Common.Services.Auth;
using Mmm.Iot.Common.Services.Config;

namespace Mmm.Iot.Config.WebService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IContainer ApplicationContainer { get; private set; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc($"v1", new OpenApiInfo { Title = "Config API", Version = "v1" });
            });

            // Setup (not enabling yet) CORS
            services.AddCors();

            var applicationInsightsOptions = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
            applicationInsightsOptions.EnableAdaptiveSampling = false;
            services.AddApplicationInsightsTelemetry(applicationInsightsOptions);

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();

            // Prepare DI container
            services.AddHttpContextAccessor();
            this.ApplicationContainer = new DependencyResolution().Setup(services, this.Configuration);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ICorsSetup corsSetup,
            IApplicationLifetime appLifetime,
            AppConfig config)
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

            var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();

            var builder = configuration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;

            // Using fixed rate sampling
            double fixedSamplingPercentage = config.Global.FixedSamplingPercentage == 0 ? 10 : config.Global.FixedSamplingPercentage;
            builder.UseSampling(fixedSamplingPercentage);
            builder.Build();

            // Enable CORS - Must be before UseMvc
            corsSetup.UseMiddleware(app);

            // app.UseAuthentication();
            app.UseMvc();
        }
    }
}