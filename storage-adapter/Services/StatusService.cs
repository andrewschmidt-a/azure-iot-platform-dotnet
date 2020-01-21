// <copyright file="StatusService.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    public class StatusService : StatusServiceBase
    {
        public StatusService(
            AppConfig config,
            IStorageClient cosmos,
            IAppConfigurationClient appConfig)
                : base(config)
        {
            Dependencies = new Dictionary<string, IStatusOperation>
            {
                { "CosmosDb", cosmos },
                { "App Config", appConfig },
            };
        }

        public override IDictionary<string, IStatusOperation> Dependencies { get; set; }
    }
}