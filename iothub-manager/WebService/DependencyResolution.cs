using System.Reflection;
using Autofac;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.IoTHubManager.Services;

namespace Mmm.Platform.IoT.IoTHubManager.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            var assembly = typeof(StatusService).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
        }
    }
}