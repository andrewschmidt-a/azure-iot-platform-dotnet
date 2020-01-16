// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            AppConfig config,
            IStorageClient cosmos) :
            base(config)
        {
            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "CosmosDb", cosmos }
            };
        }
    }
}
