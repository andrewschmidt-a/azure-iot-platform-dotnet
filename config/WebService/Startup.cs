using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Mmm.Platform.IoT.Common.Services.Auth;

namespace Mmm.Platform.IoT.Config.WebService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();

            // Prepare DI container
            services.AddHttpContextAccessor();
            this.ApplicationContainer = new DependencyResolution().Setup(services, Configuration);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

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

            // app.UseAuthentication();
            app.UseMvc();
        }
    }
}