using System.Reflection;
using Autofac;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.DeviceTelemetry.Services;
using Mmm.Platform.IoT.DeviceTelemetry.Services.External;

namespace Mmm.Platform.IoT.DeviceTelemetry.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            var assembly = typeof(StatusService).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
            builder.RegisterType<DiagnosticsClient>().As<IDiagnosticsClient>().SingleInstance();
            builder.RegisterType<StatusService>().As<IStatusService>();
        }
    }
}
