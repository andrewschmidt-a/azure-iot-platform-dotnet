using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.TenantManager.Services.External;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            AppConfig config,
            ILogger<StatusService> logger,
            IIdentityGatewayClient identityGatewayClient,
            IDeviceGroupsConfigClient deviceGroupsConfigClient,
            IStorageClient cosmosClient,
            ITableStorageClient tableStorageClient,
            IRunbookHelper RunbookHelper,
            IAppConfigurationClient appConfigClient) :
            base(config)
        {
            dependencies = new Dictionary<string, IStatusOperation>
            {
                { "CosmosDb", cosmosClient },
                { "Tenant Runbooks", RunbookHelper },
                { "Table Storage", tableStorageClient },
                { "Identity Gateway", identityGatewayClient },
                { "Config", deviceGroupsConfigClient },
                { "App Config", appConfigClient }
            };
        }
    }
}
