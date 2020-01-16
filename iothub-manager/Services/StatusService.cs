// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.External.UserManagement;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            AppConfig config,
            IStorageAdapterClient storageAdapter,
            IUserManagementClient userManagement,
            IAppConfigurationClient appConfig) :
            base(config)
        {
            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapter },
                { "User Management", userManagement },
                { "App Config", appConfig }
            };
        }
    }
}
