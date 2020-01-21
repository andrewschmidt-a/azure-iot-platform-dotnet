// <copyright file="StatusService.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.TenantManager.Services.External;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public class StatusService : StatusServiceBase
    {
        public StatusService(
            AppConfig config,
            ILogger<StatusService> logger,
            IIdentityGatewayClient identityGatewayClient,
            IDeviceGroupsConfigClient deviceGroupsConfigClient,
            IStorageClient cosmosClient,
            ITableStorageClient tableStorageClient,
            IRunbookHelper runbookHelper,
            IAppConfigurationClient appConfigClient)
                : base(config)
        {
            Dependencies = new Dictionary<string, IStatusOperation>
            {
                { "CosmosDb", cosmosClient },
                { "Tenant Runbooks", runbookHelper },
                { "Table Storage", tableStorageClient },
                { "Identity Gateway", identityGatewayClient },
                { "Config", deviceGroupsConfigClient },
                { "App Config", appConfigClient },
            };
        }

        public override IDictionary<string, IStatusOperation> Dependencies { get; set; }
    }
}