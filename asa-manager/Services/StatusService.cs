using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.AsaManager.Services
{
    public class StatusService : StatusServiceBase
    {
        private readonly ILogger<StatusService> logger;

        public StatusService(
            AppConfig config,
            ILogger<StatusService> logger,
            IIotHubManagerClient iotHubManager,
            IBlobStorageClient blobStorageClient,
            IStorageAdapterClient storageAdapterClient)
                : base(config)
        {
            this.logger = logger;
            Dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapterClient },
                { "IoTHub Manager", iotHubManager },
                { "Blob Storage", blobStorageClient }
            };
        }
    }
}
