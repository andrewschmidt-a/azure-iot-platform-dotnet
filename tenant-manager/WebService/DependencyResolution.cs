using System;
using System.Reflection;
using Autofac;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Auth;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;
using Mmm.Platform.IoT.TenantManager.Services.Runtime;
using Mmm.Platform.IoT.TenantManager.WebService.Runtime;

namespace Mmm.Platform.IoT.TenantManager.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            // Auto-wire additional assemblies
            var assembly = typeof(IServicesConfig).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            builder.Register(context => new Config(context.Resolve<ConfigData>())).As<IConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IConfig>().ClientAuthConfig).As<IClientAuthConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IConfig>().ServicesConfig).As<IServicesConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<IStorageClientConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<IAppConfigClientConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<IUserManagementClientConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<IAuthMiddlewareConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<ITableStorageClientConfig>().SingleInstance();
            builder.Register(context => GetOpenIdConnectManager(context.Resolve<IConfig>())).As<IConfigurationManager<OpenIdConnectConfiguration>>().SingleInstance();
            builder.RegisterType<CorsSetup>().As<ICorsSetup>().SingleInstance();
            // Add helper dependency types first
            builder.RegisterType<TokenHelper>().As<ITokenHelper>().SingleInstance();

            builder.RegisterType<StorageClient>().As<IStorageClient>().SingleInstance();
            builder.RegisterType<AppConfigurationHelper>().As<IAppConfigurationHelper>().SingleInstance();
            builder.RegisterType<RunbookHelper>().As<IRunbookHelper>().SingleInstance();
            builder.RegisterType<StreamAnalyticsHelper>().As<IStreamAnalyticsHelper>().SingleInstance();
            builder.RegisterType<TableStorageClient>().As<ITableStorageClient>().SingleInstance();
        }

        // Prepare the OpenId Connect configuration manager, responsibile
        // for retrieving the JWT signing keys and cache them in memory.
        // See: https://openid.net/specs/openid-connect-discovery-1_0.html#rfc.section.4
        private static IConfigurationManager<OpenIdConnectConfiguration> GetOpenIdConnectManager(IConfig config)
        {
            // Avoid starting the real OpenId Connect manager if not needed, which would
            // start throwing errors when attempting to fetch certificates.
            if (!config.ClientAuthConfig.AuthRequired)
            {
                return new StaticConfigurationManager<OpenIdConnectConfiguration>(
                    new OpenIdConnectConfiguration());
            }

            return new ConfigurationManager<OpenIdConnectConfiguration>(
                config.ClientAuthConfig.JwtIssuer + "/.well-known/openid-configuration",
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