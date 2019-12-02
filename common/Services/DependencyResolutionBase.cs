using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Runtime;

namespace Mmm.Platform.IoT.Common.Services
{
    public abstract class DependencyResolutionBase
    {
        public IContainer Setup(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);
            AutowireAssemblies(builder);
            var logger = SetupLogger(builder);
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerDependency();
            builder.RegisterType<UserManagementClient>().As<IUserManagementClient>().SingleInstance();
            builder.RegisterType<AppConfigurationHelper>().As<IAppConfigurationHelper>().SingleInstance();
            builder.RegisterType<StorageAdapterClient>().As<IStorageAdapterClient>().SingleInstance();
            var httpClient = SetupHttpClient(builder, logger);
            SetupCustomRules(builder, logger, httpClient);
            var container = builder.Build();
            RegisterFactory(container);
            return container;
        }

        private void AutowireAssemblies(ContainerBuilder builder)
        {
            var assembly = Assembly.GetEntryAssembly();
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
        }

        private ILogger SetupLogger(ContainerBuilder builder)
        {
            // Instantiate only one logger
            // TODO: read log level from configuration
            var logger = new Logger(Uptime.ProcessId, LogLevel.Debug);
            builder.RegisterInstance(logger).As<ILogger>().SingleInstance();
            return logger;
        }

        private IHttpClient SetupHttpClient(ContainerBuilder builder, ILogger logger)
        {
            // TODO: why is the HTTP client registered as a singleton? shouldn't be required
            var httpClient = new HttpClient(logger);
            builder.RegisterInstance(httpClient).As<IHttpClient>().SingleInstance();
            return httpClient;
        }

        protected abstract void SetupCustomRules(ContainerBuilder builder, ILogger logger, IHttpClient httpClient);

        private static void RegisterFactory(IContainer container)
        {
            Factory.RegisterContainer(container);
        }
    }
}