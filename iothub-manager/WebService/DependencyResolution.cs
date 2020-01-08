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
            builder.RegisterType<Services.Devices>().As<IDevices>().SingleInstance();
            builder.RegisterType<DeviceService>().As<IDeviceService>().SingleInstance();
            builder.RegisterType<Jobs>().As<IJobs>().SingleInstance();
            builder.RegisterType<DeviceProperties>().As<IDeviceProperties>().SingleInstance();
        }
    }
}