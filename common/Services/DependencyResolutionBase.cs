using Autofac;
using Autofac.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Mmm.Platform.IoT.Common.Services.Wrappers;
using Mmm.Platform.IoT.Common.Services.Config;
using KeyVault = Mmm.Platform.IoT.Common.Services.Runtime.KeyVault;

namespace Mmm.Platform.IoT.Common.Services
{
    public abstract class DependencyResolutionBase
    {
        public IContainer Setup(IServiceCollection services, IConfiguration configuration)
        {
            var builder = new ContainerBuilder();
            var appConfig = new AppConfig();
            configuration.Bind(appConfig);
            builder.RegisterInstance(appConfig).SingleInstance();
            builder.Populate(services);
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
            AutowireAssemblies(builder);
            builder.RegisterType<KeyVault>();
            builder.RegisterType<ConfigData>();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerDependency();
            builder.RegisterType<UserManagementClient>().As<IUserManagementClient>().SingleInstance();
            builder.RegisterType<AppConfigurationHelper>().As<IAppConfigurationHelper>().SingleInstance();
            builder.RegisterType<StorageAdapterClient>().As<IStorageAdapterClient>().SingleInstance();
            builder.RegisterType<ExternalRequestHelper>().As<IExternalRequestHelper>().SingleInstance();
            builder.RegisterType<GuidKeyGenerator>().As<IKeyGenerator>().SingleInstance();
            builder.RegisterType<HttpClient>().As<IHttpClient>().SingleInstance();
            SetupCustomRules(builder);
            var container = builder.Build();
            Factory.RegisterContainer(container);
            return container;
        }

        private void AutowireAssemblies(ContainerBuilder builder)
        {
            var assembly = Assembly.GetEntryAssembly();
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
        }

        protected abstract void SetupCustomRules(ContainerBuilder builder);
    }
}