using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.TenantManager.Services.External;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public class StatusService : StatusServiceBase
    {
        private readonly ILogger<StatusService> logger;

        public StatusService(
            AppConfig config,
            ILogger<StatusService> logger,
            IIdentityGatewayClient identityGatewayClient,
            IDeviceGroupsConfigClient deviceGroupsConfigClient,
            IStorageClient cosmosClient,
            ITableStorageClient tableStorageClient,
            IRunbookHelper RunbookHelper)
                : base(config)
        {
            this.logger = logger;
            dependencies = new Dictionary<string, IStatusOperation>
            {
                { "CosmosDb", cosmosClient },
                { "Tenant Runbooks", RunbookHelper },
                { "Table Storage", tableStorageClient },
                { "Identity Gateway", identityGatewayClient },
                { "Config", deviceGroupsConfigClient }
            };
        }
    }
}
