using System;
using System.Reflection;
using Autofac;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Mmm.Platform.IoT.Common.Services.Wrappers;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.AsaManager.Services.Runtime;
using Mmm.Platform.IoT.AsaManager.WebService.Runtime;

namespace Mmm.Platform.IoT.AsaManager.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            // Auto-wire additional assemblies
            var assembly = typeof(IServicesConfig).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            builder.Register(context => new Config(context.Resolve<ConfigData>())).As<IConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IConfig>().ServicesConfig).As<IServicesConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<IStorageAdapterClientConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<IBlobStorageClientConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<IIotHubManagerClientConfig>().SingleInstance();

            builder.RegisterType<StorageAdapterClient>().As<IStorageAdapterClient>().SingleInstance();
            builder.RegisterType<BlobStorageClient>().As<IBlobStorageClient>().SingleInstance();
            builder.RegisterType<IotHubManagerClient>().As<IIotHubManagerClient>().SingleInstance();
            builder.RegisterType<GuidKeyGenerator>().As<IKeyGenerator>().SingleInstance();
            
            builder.RegisterType<RulesConverter>().As<RulesConverter>().SingleInstance();
            builder.RegisterType<DeviceGroupsConverter>().As<DeviceGroupsConverter>().SingleInstance();
            builder.RegisterType<StatusService>().As<IStatusService>().SingleInstance();
        }
    }
}