// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;
using Autofac;
using Microsoft.Azure.Documents;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Wrappers;
using Mmm.Platform.IoT.StorageAdapter.Services;
using Mmm.Platform.IoT.StorageAdapter.Services.Wrappers;

namespace Mmm.Platform.IoT.StorageAdapter.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            // Auto-wire additional assemblies
            var assembly = this.GetType().GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            // By default Autofac uses a request lifetime, creating new objects
            // for each request, which is good to reduce the risk of memory
            // leaks, but not so good for the overall performance.
            builder.RegisterType<DocumentDbKeyValueContainer>().As<IKeyValueContainer>().SingleInstance();
            builder.RegisterType<DocumentClientFactory>().As<IFactory<IDocumentClient>>().SingleInstance();
            builder.RegisterType<DocumentClientExceptionChecker>().As<IExceptionChecker>().SingleInstance();
        }
    }
}
