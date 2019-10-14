using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using MMM.Azure.IoTSolutions.TenantManager.Services.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using MMM.Azure.IoTSolutions.TenantManager.Services.External;
using ILogger = MMM.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;

namespace MMM.Azure.IoTSolutions.TenantManager.Services
{
    public class StatusService : IStatusService
    {
        private ILogger _log;
        private IServicesConfig _config;
        private IIdentityGatewayClient _identityGatewayClient;
        
        private Dictionary<string, IStatusOperation> dependencies;
        
        public StatusService(
            IServicesConfig config,
            ILogger logger,
            IIdentityGatewayClient identityGatewayClient,
            CosmosHelper cosmosHelper,
            TableStorageHelper tableStorageHelper,
            TenantRunbookHelper tenantRunbookHelper)
        {
            this._log = logger;
            this._config = config;
            this._identityGatewayClient = identityGatewayClient;

            this.dependencies = new Dictionary<string, IStatusOperation>
            {
                { "CosmosDb", cosmosHelper },
                { "Tenant Runbooks", tenantRunbookHelper },
                { "Table Storage", tableStorageHelper}
            };
        }

        public async Task<StatusServiceModel> GetStatusAsync()
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

            // Check Identity Gateway as well
            var identityGatewayResult = await this._identityGatewayClient.StatusAsync();
            SetServiceStatus("Identity Gateway", identityGatewayResult, result, errors);

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
