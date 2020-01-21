// <copyright file="StatusService.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.AsaManager.Services
{
    public class StatusService : StatusServiceBase
    {
        public StatusService(
            AppConfig config,
            IIotHubManagerClient iotHubManager,
            IBlobStorageClient blobStorageClient,
            IStorageAdapterClient storageAdapterClient)
                : base(config)
        {
            Dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapterClient },
                { "IoTHub Manager", iotHubManager },
                { "Blob Storage", blobStorageClient },
            };
        }

        public override IDictionary<string, IStatusOperation> Dependencies { get; set; }
    }
}