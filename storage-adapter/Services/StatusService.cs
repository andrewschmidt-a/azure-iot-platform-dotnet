// Copyright (c) Microsoft. All rights reserved.

using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using System.Collections.Generic;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            AppConfig config,
            IStorageClient cosmos,
            IAppConfigurationClient appConfig) :
            base(config)
        {
            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "CosmosDb", cosmos },
                { "App Config", appConfig }
            };
        }
    }
}
