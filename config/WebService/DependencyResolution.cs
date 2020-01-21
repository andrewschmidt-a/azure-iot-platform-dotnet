using System.Reflection;
using Autofac;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Config.Services;

namespace Mmm.Platform.IoT.Config.WebService
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