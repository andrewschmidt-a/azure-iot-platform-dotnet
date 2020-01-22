using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.AsaManager.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            AppConfig config,
            IIotHubManagerClient iotHubManager,
            IBlobStorageClient blobStorageClient,
            IStorageAdapterClient storageAdapterClient) : base(config)
        {
            dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapterClient },
                { "IoTHub Manager", iotHubManager },
                { "Blob Storage", blobStorageClient }
            };
        }
    }
}
