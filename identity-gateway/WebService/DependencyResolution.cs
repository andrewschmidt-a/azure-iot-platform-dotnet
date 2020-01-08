using System.Reflection;
using Autofac;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services;

namespace Mmm.Platform.IoT.IdentityGateway.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            var assembly = typeof(StatusService).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
            builder.RegisterType<JwtHelpers>().As<IJwtHelpers>().InstancePerDependency();
            builder.RegisterType<AuthenticationContext>().As<IAuthenticationContext>();
            builder.RegisterType<UserSettingsContainer>().SingleInstance();
            builder.RegisterType<UserTenantContainer>().SingleInstance();
            builder.RegisterType<StatusService>().As<IStatusService>().SingleInstance();
            builder.RegisterType<RsaHelpers>().As<IRsaHelpers>().SingleInstance();
            builder.RegisterType<OpenIdProviderConfiguration>().As<IOpenIdProviderConfiguration>();
            builder.RegisterType<SendGridClientFactory>().As<SendGridClientFactory>();
        }
    }
}
