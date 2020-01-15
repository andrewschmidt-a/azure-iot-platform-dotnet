using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Microsoft.Extensions.Configuration;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            AppConfig config,
            ITableStorageClient tableStorage) :
            base(config)
        {
            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Table Storage", tableStorage }
            };
        }
    }
}
