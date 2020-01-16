// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            AppConfig config,
            IStorageAdapterClient storageAdapter,
            IUserManagementClient userManagement,
            IDevices devices) :
            base(config)
        {
            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapter },
                { "User Management", userManagement },
                { "Devices", devices }
            };
        }
    }
}
