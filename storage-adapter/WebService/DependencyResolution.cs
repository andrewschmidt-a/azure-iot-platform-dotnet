// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;
using Autofac;
using Microsoft.Azure.Documents;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Mmm.Platform.IoT.Common.Services.Wrappers;
using Mmm.Platform.IoT.StorageAdapter.Services;
using Mmm.Platform.IoT.StorageAdapter.Services.Runtime;
using Mmm.Platform.IoT.StorageAdapter.Services.Wrappers;
using Mmm.Platform.IoT.StorageAdapter.WebService.Runtime;
using Mmm.Platform.IoT.StorageAdapter.WebService.Wrappers;

namespace Mmm.Platform.IoT.StorageAdapter.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            // Auto-wire additional assemblies
            var assembly = typeof(IServicesConfig).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            builder.Register(context => { return new Config(context.Resolve<ConfigData>()); }).As<IConfig>().SingleInstance();
            builder.Register(context => { return context.Resolve<IConfig>().ServicesConfig; }).As<IServicesConfig>().SingleInstance();
            builder.Register(context => context.Resolve<IServicesConfig>()).As<IAppConfigClientConfig>().SingleInstance();

            // By default Autofac uses a request lifetime, creating new objects
            // for each request, which is good to reduce the risk of memory
            // leaks, but not so good for the overall performance.
            builder.RegisterType<DocumentDbKeyValueContainer>().As<IKeyValueContainer>().SingleInstance();
            builder.RegisterType<DocumentClientFactory>().As<IFactory<IDocumentClient>>().SingleInstance();
            builder.RegisterType<DocumentClientExceptionChecker>().As<IExceptionChecker>().SingleInstance();
            builder.RegisterType<GuidKeyGenerator>().As<IKeyGenerator>().SingleInstance();
        }
    }
}
