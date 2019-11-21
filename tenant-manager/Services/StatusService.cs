using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.TenantManager.Services.External;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;
using Mmm.Platform.IoT.TenantManager.Services.Runtime;
using Microsoft.Extensions.Logging;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public class StatusService : IStatusService
    {
        private readonly ILogger _logger;
        private IServicesConfig _config;

        private Dictionary<string, IStatusOperation> dependencies;

        public StatusService(
            IServicesConfig config,
            ILogger<StatusService> logger,
            IIdentityGatewayClient identityGatewayClient,
            IDeviceGroupsConfigClient deviceGroupsConfigClient,
            CosmosHelper cosmosHelper,
            TableStorageHelper tableStorageHelper,
            TenantRunbookHelper tenantRunbookHelper)
        {
            _logger = logger;
            this._config = config;

            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "CosmosDb", cosmosHelper },
                { "Tenant Runbooks", tenantRunbookHelper },
                { "Table Storage", tableStorageHelper },
                { "Identity Gateway", identityGatewayClient },
                { "Config", deviceGroupsConfigClient }
            };
        }

        public async Task<StatusServiceModel> GetStatusAsync(bool authRequired)
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // Loop over the IStatusOperation classes and get each status - set service status based on each response
            foreach (KeyValuePair<string, IStatusOperation> dependency in this.dependencies)
            {
                var service = dependency.Value;
                var serviceResult = await service.StatusAsync();
                SetServiceStatus(dependency.Key, serviceResult, result, errors);
            }

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }
            return result;
        }

        private void SetServiceStatus(
            string dependencyName,
            StatusResultServiceModel serviceResult,
            StatusServiceModel result,
            List<string> errors
            )
        {
            if (!serviceResult.IsHealthy)
            {
                errors.Add(dependencyName + " check failed");
                result.Status.IsHealthy = false;
            }
            result.Dependencies.Add(dependencyName, serviceResult);
        }
    }
}
