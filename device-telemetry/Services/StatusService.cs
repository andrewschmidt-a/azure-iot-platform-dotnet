// <copyright file="StatusService.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.DeviceTelemetry.Services.External;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services
{
    public class StatusService : StatusServiceBase
    {
        public StatusService(
            AppConfig config,
            IStorageClient storageClient,
            ITimeSeriesClient timeSeriesClient,
            IAsaManagerClient asaManager,
            IStorageAdapterClient storageAdapter,
            IDiagnosticsClient diagnosticsClient,
            IAppConfigurationClient appConfig)
                : base(config)
        {
            Dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapter },
                { "Storage", storageClient },
                { "Asa Manager", asaManager },
                { "Time Series", timeSeriesClient },
                { "Diagnostics", diagnosticsClient },
                { "App Config", appConfig },
            };
        }

        public override IDictionary<string, IStatusOperation> Dependencies { get; set; }
    }
}