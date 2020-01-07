using System.Reflection;
using Autofac;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;

namespace Mmm.Platform.IoT.AsaManager.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            var assembly = typeof(StatusService).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
            builder.RegisterType<BlobStorageClient>().As<IBlobStorageClient>().SingleInstance();
            builder.RegisterType<IotHubManagerClient>().As<IIotHubManagerClient>().SingleInstance();
            builder.RegisterType<RulesConverter>().As<RulesConverter>().SingleInstance();
            builder.RegisterType<DeviceGroupsConverter>().As<DeviceGroupsConverter>().SingleInstance();
            builder.RegisterType<StatusService>().As<IStatusService>().SingleInstance();
        }
    }
}