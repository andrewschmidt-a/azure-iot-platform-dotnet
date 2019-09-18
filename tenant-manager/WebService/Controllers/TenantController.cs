using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;
using Azure.ApplicationModel.Configuration;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.External;


namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private KeyVaultHelper keyVaultHelper;
        private TokenHelper tokenHelper;
        //private IIdentityGatewayClient idGatewayClient;
        private CosmosHelper cosmosHelper;


        public TenantController(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
            this.tokenHelper = new TokenHelper(this._config);
            //this.idGatewayClient = identityGatewayClient;
            string cosmosDb = this._config["TenantManagerService:CosmosDb"];
            string cosmosDbToken = this._config["TenantManagerService:cosmoskey"];
            this.cosmosHelper = new CosmosHelper(cosmosDb, cosmosDbToken);
        }

        // POST api/tenant
        [HttpPost]
        public async Task<IActionResult> Post()
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
            };
            Task.WaitAll(secretTasks);
            
            string storageAccountConnectionString = secretTasks[0].Result;
            string createIotHubWebHookUrl = secretTasks[1].Result;

            // Generate new tenant information
            string tenantGuid = Guid.NewGuid().ToString();
            string iotHubName = $"iothub-{tenantGuid.Substring(0, 8)}";
            string telemetryCollectionName = $"telemetry-{tenantGuid}";
            string twinChangeCollectionName = $"twin-change-{tenantGuid}";
            string lifecycleCollectionName = $"lifecycle-{tenantGuid}";

            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantGuid, iotHubName, telemetryCollectionName);
            await TableStorageHelper<TenantModel>.WriteToTableAsync(storageAccountConnectionString, "tenant", tenant);

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
                appConfigConnectionString = this._config["PCS_APPLICATION_CONFIGURATION"],
                setAppConfigEndpoint = this._config["TenantManagerService:setAppConfigEndpoint"],
                token = authToken
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var createIoTHubResponse = await client.PostAsync(createIotHubWebHookUrl, bodyContent);

            // Update the user table in table storage to give the requesting user an admin role to the new tenant
            var userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
            var role = @"[""admin""]";
            var userTenant = new UserTenantModel(userId, tenantGuid, role);
            await TableStorageHelper<UserTenantModel>.WriteToTableAsync(storageAccountConnectionString, "user", userTenant);

            // Update the userSettings table with the lastUsedTenant if there isn't already a lastUsedTenant
            var lastUsedTenant = TableStorageHelper<UserSettingsModel>.ReadFromTableAsync(storageAccountConnectionString, "userSettings", userId, "LastUsedTenant").Result;
            if (lastUsedTenant == null)
            {
                // Set the last used tenant to be this new tenant
                var userSettings = new UserSettingsModel(userId, "LastUsedTenant", tenantGuid);
                await TableStorageHelper<UserSettingsModel>.WriteToTableAsync(storageAccountConnectionString, "userSettings", userSettings);
            }

            // Write tenant info cosmos db collection name to app config
            var appConfgiClient = new ConfigurationClient(this._config["PCS_APPLICATION_CONFIGURATION"]);
            var PcsCollectionSetting = new ConfigurationSetting($"tenant:{tenantGuid}:pcs-collection", $"pcs-{tenantGuid}");
            appConfgiClient.Set(PcsCollectionSetting);

            // Write telemetry cosmos db collection name to app config
            var telemetryCollectionSetting = new ConfigurationSetting($"tenant:{tenantGuid}:telemetry-collection", $"telemetry-{tenantGuid}");
            appConfgiClient.Set(telemetryCollectionSetting);

            var response = new {
                message = "Your tenant is being created.",
                tenantId = tenantGuid
            };

            return Ok(JsonConvert.SerializeObject(response));
        }

        // GET api/tenant/<tenantId>
        [HttpGet("{tenantId}")]
        public async Task<ActionResult<TenantModel>> Get(string tenantId)
        {
            /* Returns information for a tenant */

            // Load variables from key vault
            var storageAccountConnectionString = this.keyVaultHelper.getSecretAsync("storageAccountConnectionString").Result;

            // Verify that the user has access to the specified tenant
            var userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
            var userTenant = await TableStorageHelper<UserTenantModel>.ReadFromTableAsync(storageAccountConnectionString, "user", userId, tenantId);

            if (userTenant == null) {
                // User does not have access
                return Unauthorized();
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await TableStorageHelper<TenantModel>.ReadFromTableAsync(storageAccountConnectionString, "tenant", partitionKey, tenantId);            

            return Ok(tenant);
        }

         // DELETE api/tenantready/<tenantId>
        [HttpDelete("{tenantId}")]
        public async Task<IActionResult> DeleteAsync(string tenantId)
        {
            string subscriptionId = this._config["Global:subscriptionId"];
            string rgName = this._config["Global:resourceGroup"];
            string location = this._config["Global:location"];

            // Load secrets from key vault
            var secretTasks = new Task<string>[] {
                this.keyVaultHelper.getSecretAsync("storageAccountConnectionString"),
                this.keyVaultHelper.getSecretAsync("deleteIotHubWebHookUrl")
            };
            Task.WaitAll(secretTasks);

            string storageAccountConnectionString = secretTasks[0].Result;
            string deleteIotHubWebHookUrl = secretTasks[1].Result;

            // Verify that the user has access to the specified tenant
            var userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
            var userTenant = TableStorageHelper<UserTenantModel>.ReadFromTableAsync(storageAccountConnectionString, "user", userId, tenantId).Result;

            if (userTenant == null)
            {
                // User does not have access
                return Unauthorized();
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            try
            { 
                TenantModel tenant = TableStorageHelper<TenantModel>.ReadFromTableAsync(storageAccountConnectionString, "tenant", partitionKey, tenantId).Result;
                await TableStorageHelper<TenantModel>.DeleteEntityAsync(storageAccountConnectionString, "tenant", tenant);
            }
            catch (Exception e)
            {
                LogException(e);
            }

            HttpClient client = new HttpClient();
            var authToken = this.tokenHelper.GetServicePrincipleToken();

            // Gather tenant information
            string tenantGuid = tenantId;
            string iotHubName = "iothub-" + tenantGuid.Substring(0, 8);
            string telemetryCollectionName = "telemetry-" + tenantGuid;
            string twinChangeCollectionName = "twin-change-" + tenantGuid;
            string lifecycleCollectionName = "lifecycle-" + tenantGuid;
            string pcsStorageName = "pcsCollection-" + tenantGuid;
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
				CosmosDb = this._config["TenantManagerService:CosmosDb"],
                appConfigConnectionString = this._config["PCS_APPLICATION_CONFIGURATION"],
                setAppConfigEndpoint = this._config["TenantManagerService:setAppConfigEndpoint"],
                token = authToken
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            //Grab information about cosmosdb
            string dbIdTms = this._config["TenantManagerService:databaseName"];
            string dbIdStorage = this._config["StorageAdapter:documentDb"];

            try
            {
                await client.PostAsync(deleteIotHubWebHookUrl, bodyContent);
            }catch(Exception e)
            {
                LogException(e);
            }
            try
            {
                await cosmosHelper.DeleteCosmosDbCollection(dbIdTms, telemetryCollectionName);
                await cosmosHelper.DeleteCosmosDbCollection(dbIdTms, twinChangeCollectionName);
                await cosmosHelper.DeleteCosmosDbCollection(dbIdTms, lifecycleCollectionName);
                await cosmosHelper.DeleteCosmosDbCollection(dbIdStorage, pcsStorageName);
            }catch(Exception e)
            {
                LogException(e);
            }
            var response = await client.PostAsync(deleteIotHubWebHookUrl, bodyContent);

            return Ok();
        }
        private void LogException(Exception e)
        {
            Exception baseException = e.GetBaseException();
            Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
        }
    }
}
