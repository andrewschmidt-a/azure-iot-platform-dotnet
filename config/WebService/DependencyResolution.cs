// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;
using Autofac;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Config.Services;
using Mmm.Platform.IoT.Config.Services.Models.Actions;
using Mmm.Platform.IoT.Config.Services.External;

namespace Mmm.Platform.IoT.Config.WebService
{
    public class DependencyResolution : DependencyResolutionBase
    {
        protected override void SetupCustomRules(ContainerBuilder builder)
        {
            var assembly = typeof(StatusService).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
            builder.RegisterType<Storage>().As<IStorage>().SingleInstance();
            builder.RegisterType<AzureResourceManagerClient>().As<IAzureResourceManagerClient>().SingleInstance();
            builder.RegisterType<EmailActionSettings>().As<EmailActionSettings>().SingleInstance();
            builder.RegisterType<Actions>().As<IActions>().SingleInstance();
            builder.RegisterType<StatusService>().As<IStatusService>();
        }
    }
}
