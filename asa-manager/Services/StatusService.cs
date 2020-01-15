using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.AsaManager.Services
{
    public class StatusService : StatusServiceBase
    {
        private readonly ILogger<StatusService> _logger;

        public StatusService(
            AppConfig config,
            ILogger<StatusService> logger,
            IIotHubManagerClient iotHubManager,
            IBlobStorageClient blobStorageClient,
            IStorageAdapterClient storageAdapterClient) : base(config)
        {
            _logger = logger;
            dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapterClient },
                { "IoTHub Manager", iotHubManager },
                { "Blob Storage", blobStorageClient }
            };
        }
    }
}
