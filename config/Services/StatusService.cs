// <copyright file="StatusService.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Config.Services.External;

namespace Mmm.Platform.IoT.Config.Services
{
    public class StatusService : StatusServiceBase
    {
        public StatusService(
            AppConfig config,
            IAsaManagerClient asaManager,
            IStorageAdapterClient storageAdapter,
            IDeviceTelemetryClient deviceTelemetry,
            IDeviceSimulationClient deviceSimulation)
                : base(config)
        {
            this.Dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Storage Adapter", storageAdapter },
                { "Device Telemetry", deviceTelemetry },
                { "Device Simulation", deviceSimulation },
                { "Asa Manager", asaManager },
            };
        }

        public override IDictionary<string, IStatusOperation> Dependencies { get; set; }
    }
}