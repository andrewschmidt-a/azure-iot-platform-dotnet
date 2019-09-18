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
        // _config keys
        private const string APP_CONFIGURATION_KEY = "PCS_APPLICATION_CONFIGURATION";
        private const string GLOBAL_KEY = "Global";
        private const string STORAGE_ADAPTER_DOCUMENT_DB_KEY = "StorageAdapter:documentDb";
        private const string TENANT_MANAGEMENT_KEY = "TenantManagerService:";
        private const string EVENT_HUB_CONN_STRING_SUFFIX = "EventHubConnString";
        private const string TELEMETRY_EVENT_HUB_CONN_STRING_KEY = TENANT_MANAGEMENT_KEY + "telemetry" + EVENT_HUB_CONN_STRING_SUFFIX;
        private const string LIFECYCLE_EVENT_HUB_CONN_STRING_KEY = TENANT_MANAGEMENT_KEY + "lifecycle" + EVENT_HUB_CONN_STRING_SUFFIX;
        private const string TWIN_CHANGE_EVENT_HUB_CONN_STRING_KEY = TENANT_MANAGEMENT_KEY + "twinChange" + EVENT_HUB_CONN_STRING_SUFFIX;
        private const string COSMOS_DB_KEY = TENANT_MANAGEMENT_KEY + "CosmosDb";
        private const string COSMOS_KEY = TENANT_MANAGEMENT_KEY + "cosmoskey";
        private const string DATABASE_KEY = TENANT_MANAGEMENT_KEY + "databaseName";
        private const string APP_CONFIG_ENDPOINT_KEY = TENANT_MANAGEMENT_KEY + "setAppConfigEndpoint";

        // config keys specific to GetSecretAsync from keyvault
        private const string STORAGE_ACCOUNT_CONNECTION_STRING_KEY = "storageAccountConnectionString";
        private const string DELETE_IOTHUB_URL_KEY = "deleteIotHubWebHookUrl";
        private const string CREATE_IOTHUB_URL_KEY = "createIotHubWebHookUrl";

        // table storage table ids
        private const string TENANT_TABLE_ID = "tenant";
        private const string USER_TABLE_ID = "user";
        private const string USER_SETTINGS_TABLE_ID = "userSettings";

        // injected and created attribute
        private IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private KeyVaultHelper keyVaultHelper;
        private TokenHelper tokenHelper;
        private CosmosHelper cosmosHelper;

        // collection and iothub naming 
        private string iotHubNameFormat = "iothub-{0}";  // format with a guid
        private string appConfigCollectionKeyFormat = "tenant:{0}:{1}-collection";  // format with a guid and collection name
        private List<string> tenantCollections = new List<string>{"telemetry", "twin-change", "lifecycle", "pcs"};

        public TenantController(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
            this.tokenHelper = new TokenHelper(this._config);
            string cosmosDb = this._config[COSMOS_DB_KEY];
            string cosmosDbToken = this._config[COSMOS_KEY];
            this.cosmosHelper = new CosmosHelper(cosmosDb, cosmosDbToken);
        }

        public Dictionary<string, string> azureInfo
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "subscriptionId", this._config[$"{GLOBAL_KEY}:subscriptionId"] },
                    { "resourceGroup", this._config[$"{GLOBAL_KEY}:resourceGroup"] },
                    { "location", this._config[$"{GLOBAL_KEY}:location"]}
                };
            }
        }

        // POST api/tenant
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            /* Creates a new tenant */
            // Load secrets from key vault
            var secretTasks = new Task<string>[] {
                this.keyVaultHelper.getSecretAsync(STORAGE_ACCOUNT_CONNECTION_STRING_KEY),
                this.keyVaultHelper.getSecretAsync(CREATE_IOTHUB_URL_KEY),
            };
            Task.WaitAll(secretTasks);
            
            string storageAccountConnectionString = secretTasks[0].Result;
            string createIotHubWebHookUrl = secretTasks[1].Result;

            // Generate new tenant information
            string tenantGuid = Guid.NewGuid().ToString();
            string iotHubName = String.Format(this.iotHubNameFormat, tenantGuid.Substring(0, 8));

            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantGuid, iotHubName);
            await TableStorageHelper<TenantModel>.WriteToTableAsync(storageAccountConnectionString, TENANT_TABLE_ID, tenant);

            // Trigger run book to create a new IoT Hub
            HttpClient client = new HttpClient();
            var authToken = this.tokenHelper.GetServicePrincipleToken();
            var azureInfo = this.azureInfo;

            var requestBody = new
            {   
                tenantId = tenantGuid,
                iotHubName = iotHubName,
                location = azureInfo["location"],
                subscriptionId = azureInfo["subscriptionId"],
                resourceGroup = azureInfo["resourceGroup"],
                telemetryEventHubConnString = this._config[TELEMETRY_EVENT_HUB_CONN_STRING_KEY],
                twinChangeEventHubConnString = this._config[TWIN_CHANGE_EVENT_HUB_CONN_STRING_KEY],
                lifecycleEventHubConnString = this._config[LIFECYCLE_EVENT_HUB_CONN_STRING_KEY],
                appConfigConnectionString = this._config[APP_CONFIGURATION_KEY],
                setAppConfigEndpoint = this._config[APP_CONFIG_ENDPOINT_KEY],
                token = authToken
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var createIoTHubResponse = await client.PostAsync(createIotHubWebHookUrl, bodyContent);

            // Update the user table in table storage to the requesting user an admin role to the new tenant
            var userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
            var role = @"[""admin""]";
            var userTenant = new UserTenantModel(userId, tenantGuid, role);
            await TableStorageHelper<UserTenantModel>.WriteToTableAsync(storageAccountConnectionString, USER_TABLE_ID, userTenant);

            // Update the userSettings table with the lastUsedTenant if there isn't already a lastUsedTenant
            // TODO: Use IDGateway
            var lastUsedTenant = TableStorageHelper<UserSettingsModel>.ReadFromTableAsync(storageAccountConnectionString, USER_SETTINGS_TABLE_ID, userId, "LastUsedTenant").Result;
            if (lastUsedTenant == null)
            {
                // Set the last used tenant to be this new tenant
                var userSettings = new UserSettingsModel(userId, "LastUsedTenant", tenantGuid);
                await TableStorageHelper<UserSettingsModel>.WriteToTableAsync(storageAccountConnectionString, USER_SETTINGS_TABLE_ID, userSettings);
            }

            // Write tenant info cosmos db collection name to app config
            var appConfigClient = new ConfigurationClient(this._config[APP_CONFIGURATION_KEY]);
            foreach (string collection in this.tenantCollections)
            {
                string collectionKey = String.Format(this.appConfigCollectionKeyFormat, tenantGuid, collection);
                string collectionId = $"{collection}-{tenantGuid}";
                var collectionSetting = new ConfigurationSetting(collectionKey, collectionId);
                appConfigClient.Set(collectionSetting);
            }

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
            var storageAccountConnectionString = this.keyVaultHelper.getSecretAsync(STORAGE_ACCOUNT_CONNECTION_STRING_KEY).Result;

            // Verify that the user has access to the specified tenant
            // TODO: Use IDGateway
            var userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
            var userTenant = await TableStorageHelper<UserTenantModel>.ReadFromTableAsync(storageAccountConnectionString, USER_TABLE_ID, userId, tenantId);

            if (userTenant == null) {
                // User does not have access
                return Unauthorized();
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await TableStorageHelper<TenantModel>.ReadFromTableAsync(storageAccountConnectionString, TENANT_TABLE_ID, partitionKey, tenantId);            

            return Ok(tenant);
        }

         // DELETE api/tenantready/<tenantId>
        [HttpDelete("{tenantId}")]
        public async Task<IActionResult> DeleteAsync(string tenantId)
        {
            // Load secrets from key vault
            var secretTasks = new Task<string>[] {
                this.keyVaultHelper.getSecretAsync(STORAGE_ACCOUNT_CONNECTION_STRING_KEY),
                this.keyVaultHelper.getSecretAsync(DELETE_IOTHUB_URL_KEY)
            };
            Task.WaitAll(secretTasks);

            string storageAccountConnectionString = secretTasks[0].Result;
            string deleteIotHubWebHookUrl = secretTasks[1].Result;

            // Verify that the user has access to the specified tenant
            // TODO: Use IDGateway
            var userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
            var userTenant = TableStorageHelper<UserTenantModel>.ReadFromTableAsync(storageAccountConnectionString, USER_TABLE_ID, userId, tenantId).Result;

            if (userTenant == null)
            {
                // User does not have access
                return Unauthorized();
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            try
            { 
                TenantModel tenant = TableStorageHelper<TenantModel>.ReadFromTableAsync(storageAccountConnectionString, TENANT_TABLE_ID, partitionKey, tenantId).Result;
                await TableStorageHelper<TenantModel>.DeleteEntityAsync(storageAccountConnectionString, TENANT_TABLE_ID, tenant);
            }
            catch (Exception e)
            {
                LogException(e);
            }

            HttpClient client = new HttpClient();
            var authToken = this.tokenHelper.GetServicePrincipleToken();

            // Gather tenant information
            string tenantGuid = tenantId;
            string iotHubName = String.Format(this.iotHubNameFormat, tenantGuid.Substring(0, 8));
            var azureInfo = this.azureInfo;

            //trigger delete iothub runbook
            var requestBody = new
            {
                tenantId = tenantGuid,
                iotHubName = iotHubName,
                location = azureInfo["location"],
                subscriptionId = azureInfo["subscriptionId"],
                resourceGroup = azureInfo["resourceGroup"],
                telemetryEventHubConnString = this._config[TELEMETRY_EVENT_HUB_CONN_STRING_KEY],
                twinChangeEventHubConnString = this._config[TWIN_CHANGE_EVENT_HUB_CONN_STRING_KEY],
                lifecycleEventHubConnString = this._config[LIFECYCLE_EVENT_HUB_CONN_STRING_KEY],
				CosmosDb = this._config[COSMOS_DB_KEY],
                appConfigConnectionString = this._config[APP_CONFIGURATION_KEY],
                setAppConfigEndpoint = this._config[APP_CONFIG_ENDPOINT_KEY],
                token = authToken
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            string dbIdTms = this._config[DATABASE_KEY];
            string dbIdStorage = this._config[STORAGE_ADAPTER_DOCUMENT_DB_KEY];

            // Delete iotHub
            try
            {
                await client.PostAsync(deleteIotHubWebHookUrl, bodyContent);
            }
            catch (Exception e)
            {
                LogException(e);
            }

            // Delete collections
            try
            {
                var appConfigClient = new ConfigurationClient(this._config[APP_CONFIGURATION_KEY]);
                foreach (string collection in this.tenantCollections)
                {
                    string collectionKey = String.Format(this.appConfigCollectionKeyFormat, tenantGuid, collection);
                    string collectionId = appConfigClient.Get(collectionKey).Value.Value;
                    // pcs colleciton uses a different database than the other collections
                    string databaseId = collection == "pcs" ? dbIdStorage : dbIdTms;
                    await cosmosHelper.DeleteCosmosDbCollection(databaseId, collectionId);
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }

            return Ok();
        }

        private void LogException(Exception e)
        {
            Exception baseException = e.GetBaseException();
            Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
        }
    }
}
