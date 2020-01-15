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
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using Mmm.Platform.IoT.Common.Services.Auth;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;

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
            builder.RegisterType<UserManagementClient>().As<IUserManagementClient>().SingleInstance();
            builder.RegisterType<AppConfigurationHelper>().As<IAppConfigurationHelper>().SingleInstance();
            builder.RegisterType<StorageAdapterClient>().As<IStorageAdapterClient>().SingleInstance();
            builder.RegisterType<ExternalRequestHelper>().As<IExternalRequestHelper>().SingleInstance();
            builder.RegisterType<GuidKeyGenerator>().As<IKeyGenerator>().SingleInstance();
            builder.RegisterType<HttpClient>().As<IHttpClient>().SingleInstance();
            builder.Register(context => GetOpenIdConnectManager(context.Resolve<AppConfig>())).As<IConfigurationManager<OpenIdConnectConfiguration>>().SingleInstance();
            builder.RegisterType<CorsSetup>().As<ICorsSetup>().SingleInstance();
            builder.RegisterType<StorageClient>().As<IStorageClient>().SingleInstance();
            builder.RegisterType<AsaManagerClient>().As<IAsaManagerClient>().SingleInstance();
            builder.RegisterType<TimeSeriesClient>().As<ITimeSeriesClient>().SingleInstance();
            builder.RegisterType<TableStorageClient>().As<ITableStorageClient>().SingleInstance();
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

        // Prepare the OpenId Connect configuration manager, responsibile
        // for retrieving the JWT signing keys and cache them in memory.
        // See: https://openid.net/specs/openid-connect-discovery-1_0.html#rfc.section.4
        private static IConfigurationManager<OpenIdConnectConfiguration> GetOpenIdConnectManager(AppConfig config)
        {
            // Avoid starting the real OpenId Connect manager if not needed, which would
            // start throwing errors when attempting to fetch certificates.
            if (!config.Global.AuthRequired)
            {
                return new StaticConfigurationManager<OpenIdConnectConfiguration>(
                    new OpenIdConnectConfiguration());
            }

            return new ConfigurationManager<OpenIdConnectConfiguration>(
                config.Global.ClientAuth.Jwt.AuthIssuer + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever())
            {
                // How often the list of keys in memory is refreshed. Default is 24 hours.
                AutomaticRefreshInterval = TimeSpan.FromHours(6),

                // The minimum time between retrievals, in the event that a retrieval
                // failed, or that a refresh is explicitly requested. Default is 30 seconds.
                RefreshInterval = TimeSpan.FromMinutes(1)
            };
        }
    }
}