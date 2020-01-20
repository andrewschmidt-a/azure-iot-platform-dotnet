using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.IdentityGateway.Services.External;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public class StatusService : StatusServiceBase
    {
        public override IDictionary<string, IStatusOperation> dependencies { get; set; }

        public StatusService(
            AppConfig config,
            ITableStorageClient tableStorage,
            IAzureB2cClient b2cClient) :
            base(config)
        {
            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "Table Storage", tableStorage },
                { "AzureB2C", b2cClient }
            };
        }
    }
}
