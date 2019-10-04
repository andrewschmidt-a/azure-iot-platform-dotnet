using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using MMM.Azure.IoTSolutions.TenantManager.Services.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using MMM.Azure.IoTSolutions.TenantManager.Services.External;
using ILogger = MMM.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;

namespace MMM.Azure.IoTSolutions.TenantManager.Services
{
    public class StatusService : IStatusService
    {
        const string AzureB2CBaseUri = "Global:AzureB2CBaseUri";
        private const string TENANT_MANAGEMENT_KEY = "TenantManagerService:";
        private const string COSMOS_DB_KEY = TENANT_MANAGEMENT_KEY + "CosmosDb";
        private const string COSMOS_KEY = TENANT_MANAGEMENT_KEY + "cosmoskey";
        private const string STORAGE_ACCOUNT_CONNECTION_STRING_KEY = "storageAccountConnectionString";

        private ILogger _log;
        private IConfiguration _config;
        private IIdentityGatewayClient _identityGatewayClient;
        
        public StatusService(IConfiguration config, ILogger logger, IIdentityGatewayClient identityGatewayClient)
        {
            this._log = logger;
            this._config = config;
            this._identityGatewayClient = identityGatewayClient;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            KeyVaultHelper keyVaultHelper = new KeyVaultHelper(this._config);
            var keyVaultResult = await keyVaultHelper.StatusAsync();
            SetServiceStatus("KeyVault", keyVaultResult, result, errors);

            string cosmosDb = this._config[COSMOS_DB_KEY];
            string cosmosDbToken = this._config[COSMOS_KEY];
            CosmosHelper cosmosHelper = new CosmosHelper(cosmosDb, cosmosDbToken);
            var cosmosResult = await cosmosHelper.StatusAsync();
            SetServiceStatus("CosmosDb", cosmosResult, result, errors);

            string storageAccountConnectionString = await keyVaultHelper.GetSecretAsync(STORAGE_ACCOUNT_CONNECTION_STRING_KEY);
            TableStorageHelper tableStorageHelper = new TableStorageHelper(storageAccountConnectionString);
            var tableStorageResult = await tableStorageHelper.StatusAsync();
            SetServiceStatus("Table Storage", tableStorageResult, result, errors);

            TenantRunbookHelper runbookHelper = new TenantRunbookHelper(this._config);
            var runbookResult = await runbookHelper.StatusAsync();
            SetServiceStatus("Tenant Runbooks", runbookResult, result, errors);

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
