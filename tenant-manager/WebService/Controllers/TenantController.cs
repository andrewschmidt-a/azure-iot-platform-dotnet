using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.ApplicationModel.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private IConfiguration _config;
        private KeyVaultHelper keyVaultHelper;
        private TokenHelper tokenHelper;

        public TenantController(IConfiguration config)
        {
            this._config = config;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
            this.tokenHelper = new TokenHelper(this._config);
        }

        // POST api/tenant
        [HttpPost]
        public async Task<string> PostAsync()
        {
            /* Creates a new tenant */


            // Load variables from app config
            string subscriptionId = this._config["Global:subscriptionId"];
            string rgName = this._config["Global:resourceGroup"];
            string location = this._config["Global:location"];
            
            // Load secrets from key vault
            var secretTasks = new Task<string>[] {
                this.keyVaultHelper.getSecretAsync("storageAccountConnectionString"),
                this.keyVaultHelper.getSecretAsync("createIotHubWebHookUrl"),
                this.keyVaultHelper.getSecretAsync("updateFunctionsWebHookUrl"),
                this.keyVaultHelper.getSecretAsync("deleteIotHubWebHookUrl")
            };
            Task.WaitAll(secretTasks);
            
            string storageAccountConnectionString = secretTasks[0].Result;
            string createIotHubWebHookUrl = secretTasks[1].Result;
            string updateFunctionsWebHookUrl = secretTasks[2].Result;
            string deleteIotHubWebHookUrl = secretTasks[3].Result;

            // Generate new tenant information
            string tenantGuid = Guid.NewGuid().ToString();
            string iotHubName = "iothub-" + tenantGuid.Substring(0, 8);
            string telemetryCollectionName = "telemetry-" + tenantGuid.Substring(0, 8);
            string twinChangeCollectionName = "twin-change-" + tenantGuid.Substring(0, 8);
            string lifecycleCollectionName = "lifecycle-" + tenantGuid.Substring(0, 8);



            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantGuid, iotHubName, telemetryCollectionName);
            await TenantTableHelper.WriteNewTenantToTableAsync(storageAccountConnectionString, "tenant", tenant);

            // Trigger run book to create a new IoT Hub
            HttpClient client = new HttpClient();
            var authToken = this.tokenHelper.GetServicePrincipleToken();

            var requestBody = new
            {   
                tenantId = tenantGuid,
                iotHubName = iotHubName,
                location = location,
                subscriptionId = subscriptionId,
                resourceGroup = rgName,
                telemetryEventHubConnString = this._config["TenantManagerService:telemetryEventHubConnString"],
                twinChangeEventHubConnString = this._config["TenantManagerService:twinChangeEventHubConnString"],
                lifecycleEventHubConnString = this._config["TenantManagerService:lifecycleEventHubConnString"],
                appConfigConnectionString = this._config["appConfigConnectionString"],
                setAppConfigEndpoint = this._config["TenantManagerService:setAppConfigEndpoint"],
                token = authToken
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(createIotHubWebHookUrl, bodyContent);
            // Trigger run book to update the azure functions
            var requestBody2 = new
            {
                type = "create",
                tenantId = tenantGuid,
                resourceGroup = rgName,
                automationAccountName = this._config["TenantManagerService:automationAccountName"],
                cosmosConnectionSetting = this._config["TenantManagerService:cosmosConnectionSetting"],
                telemetryFunctionUrl = this._config["TenantManagerService:telemetryFunctionUri"],
                twinChangeFunctionUrl = this._config["TenantManagerService:twinChangeFunctionUri"],
                lifecycleFunctionUrl = this._config["TenantManagerService:lifecycleFunctionUri"],
                telemetryFunctionName = this._config["TenantManagerService:telemetryFunctionName"],
                twinChangeFunctionName = this._config["TenantManagerService:twinChangeFunctionName"],
                lifecycleFunctionName = this._config["TenantManagerService:lifecycleFunctionName"],
                telemetryCollectionName = telemetryCollectionName,
                twinChangeCollectionName = twinChangeCollectionName,
                lifecycleCollectionName = lifecycleCollectionName,
                storageAccount = this._config["StorageAccount:name"],
                databaseName = this._config["TenantManagerService:databaseName"],
                tableName = this._config["TenantManagerService:tableName"],
                token = authToken
            };

            bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody2), Encoding.UTF8, "application/json");
            await client.PostAsync(updateFunctionsWebHookUrl, bodyContent);

            return "Your tenant is being created. Your tenant GUID is: " + tenantGuid;
        }

        // GET api/tenantready/<tenantId>
        [HttpGet("{tenantId}")]
        public TenantModel Get(string tenantId)
        {
            /* Returns information for a tenant */

            // Load variables from key vault
            var storageAccountConnectionString = this.keyVaultHelper.getSecretAsync("storageAccountConnectionString").Result;

            // Load the tenant from table storage
            TenantModel tenant = TenantTableHelper.ReadTenantFromTableAsync(storageAccountConnectionString, "tenant", tenantId).Result;            

            return tenant;
        }



         // DELETE api/tenantready/<tenantId>
        [HttpDelete("{tenantId}")]
        public async Task DeleteAsync(string tenantId)
        {
            string subscriptionId = this._config["Global:subscriptionId"];
            string rgName = this._config["Global:resourceGroup"];
            string location = this._config["Global:location"];

            // Load secrets from key vault
            var secretTasks = new Task<string>[] {
                this.keyVaultHelper.getSecretAsync("storageAccountConnectionString"),
                this.keyVaultHelper.getSecretAsync("updateFunctionsWebHookUrl"),
                this.keyVaultHelper.getSecretAsync("deleteIotHubWebHookUrl")
            };
            Task.WaitAll(secretTasks);

            string storageAccountConnectionString = secretTasks[0].Result;
            string updateFunctionsWebHookUrl = secretTasks[1].Result;
            string deleteIotHubWebHookUrl = secretTasks[2].Result;

            // Load the tenant from table storage
            TenantModel tenant = TenantTableHelper.ReadTenantFromTableAsync(storageAccountConnectionString, "tenant", tenantId).Result;
            await TenantTableHelper.DeleteEntityAsync(storageAccountConnectionString, "tenant", tenant);

            HttpClient client = new HttpClient();
            var authToken = this.tokenHelper.GetServicePrincipleToken();


            // Gather tenant information
            string tenantGuid = tenantId;
            string iotHubName = "iothub-" + tenantGuid.Substring(0, 8);
            string telemetryCollectionName = "telemetry-" + tenantGuid.Substring(0, 8);
            string twinChangeCollectionName = "twin-change-" + tenantGuid.Substring(0, 8);
            string lifecycleCollectionName = "lifecycle-" + tenantGuid.Substring(0, 8);

            //trigger delete iothub runbook
            var requestBody = new
            {
                tenantId = tenantGuid,
                iotHubName = iotHubName,
                location = location,
                subscriptionId = subscriptionId,
                resourceGroup = rgName,
                telemetryEventHubConnString = this._config["TenantManagerService:telemetryEventHubConnString"],
                twinChangeEventHubConnString = this._config["TenantManagerService:twinChangeEventHubConnString"],
                lifecycleEventHubConnString = this._config["TenantManagerService:lifecycleEventHubConnString"],
                appConfigConnectionString = this._config["appConfigConnectionString"],
                setAppConfigEndpoint = this._config["TenantManagerService:setAppConfigEndpoint"],
                token = authToken
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");


            var response = await client.PostAsync(deleteIotHubWebHookUrl, bodyContent);

            //trigger remove bindings from azure functions runbook
            var requestBody2 = new
            {
                type = "delete",
                tenantId = tenantGuid,
                resourceGroup = rgName,
                automationAccountName = this._config["TenantManagerService:automationAccountName"],
                cosmosConnectionSetting = this._config["TenantManagerService:cosmosConnectionSetting"],
                telemetryFunctionUrl = this._config["TenantManagerService:telemetryFunctionUri"],
                twinChangeFunctionUrl = this._config["TenantManagerService:twinChangeFunctionUri"],
                lifecycleFunctionUrl = this._config["TenantManagerService:lifecycleFunctionUri"],
                telemetryFunctionName = this._config["TenantManagerService:telemetryFunctionName"],
                twinChangeFunctionName = this._config["TenantManagerService:twinChangeFunctionName"],
                lifecycleFunctionName = this._config["TenantManagerService:lifecycleFunctionName"],
                telemetryCollectionName = telemetryCollectionName,
                twinChangeCollectionName = twinChangeCollectionName,
                lifecycleCollectionName = lifecycleCollectionName,
                storageAccount = this._config["StorageAccount:name"],
                databaseName = this._config["TenantManagerService:databaseName"],
                tableName = this._config["TenantManagerService:tableName"],
                token = authToken
            };

            bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody2), Encoding.UTF8, "application/json");
            await client.PostAsync(updateFunctionsWebHookUrl, bodyContent);

        }
    }
}
