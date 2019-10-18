using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService
{
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public IContainer ApplicationContainer { get; private set; } 

        public Startup(IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder();
#if DEBUG
            builder.AddIniFile("appsettings.ini", optional: false, reloadOnChange: true);
#endif
            builder.AddEnvironmentVariables();
            var settings = builder.Build();
            builder.AddAzureAppConfiguration(settings["PCS_APPLICATION_CONFIGURATION"]);
            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddControllersAsServices();

            this.ApplicationContainer = DependencyResolution.Setup(services);

            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Check for Authorization header before dispatching requests
            app.UseMiddleware<AuthMiddleware>();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
