// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Reflection;
using Autofac;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Auth;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Config.Services;
using Mmm.Platform.IoT.Config.Services.Models.Actions;
using Mmm.Platform.IoT.Config.Services.External;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.Config.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            // Auto-wire additional assemblies
            var assembly = typeof(StatusService).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            builder.Register(context => GetOpenIdConnectManager(context.Resolve<AppConfig>())).As<IConfigurationManager<OpenIdConnectConfiguration>>().SingleInstance();
            builder.RegisterType<CorsSetup>().As<ICorsSetup>().SingleInstance();

            // By default Autofac uses a request lifetime, creating new objects
            // for each request, which is good to reduce the risk of memory
            // leaks, but not so good for the overall performance.
            builder.RegisterType<Storage>().As<IStorage>().SingleInstance();
            builder.RegisterType<AsaManagerClient>().As<IAsaManagerClient>().SingleInstance();
            builder.RegisterType<AzureResourceManagerClient>().As<IAzureResourceManagerClient>().SingleInstance();
            builder.RegisterType<EmailActionSettings>().As<EmailActionSettings>().SingleInstance();
            builder.RegisterType<Actions>().As<IActions>().SingleInstance();
            builder.RegisterType<StatusService>().As<IStatusService>();
        }

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
