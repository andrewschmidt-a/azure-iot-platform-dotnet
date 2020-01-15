// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Config.Services.External;

namespace Mmm.Platform.IoT.Config.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            ILogger<StatusService> logger,
            IHttpClient httpClient,
            AppConfig config,
            IAsaManagerClient asaManager,
            IStorageAdapterClient storageAdapter,
            IDeviceTelemetryClient deviceTelemetry,
            IDeviceSimulationClient deviceSimulation) :
            base(config)
        {
            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapter },
                { "Device Telemetry", deviceTelemetry },
                { "Device Simulation", deviceSimulation },
                { "Asa Manager", asaManager }
            };
        }
    }
}
