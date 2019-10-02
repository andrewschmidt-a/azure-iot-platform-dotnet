using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Filters;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;
using Microsoft.Azure.Documents;
using Azure.ApplicationModel.Configuration;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Exceptions;
using ILogger = Microsoft.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.External;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class TenantController : ControllerBase
    {
        // _config keys
        private const string APP_CONFIGURATION_KEY = "PCS_APPLICATION_CONFIGURATION";
        private const string GLOBAL_KEY = "Global";
        private const string STORAGE_ADAPTER_DOCUMENT_DB_KEY = "StorageAdapter:documentDb";
        private const string TENANT_MANAGEMENT_KEY = "TenantManagerService:";
        private const string COSMOS_DB_KEY = TENANT_MANAGEMENT_KEY + "CosmosDb";
        private const string COSMOS_KEY = TENANT_MANAGEMENT_KEY + "cosmoskey";
        private const string DATABASE_KEY = TENANT_MANAGEMENT_KEY + "databaseName";

        // config keys specific to GetSecretAsync from keyvault
        private const string STORAGE_ACCOUNT_CONNECTION_STRING_KEY = "storageAccountConnectionString";
        private const string DELETE_IOTHUB_URL_KEY = "deleteIotHubWebHookUrl";
        private const string CREATE_IOTHUB_URL_KEY = "createIotHubWebHookUrl";

        // table storage table ids
        private const string TENANT_TABLE_ID = "tenant";
        private const string USER_TABLE_ID = "user";
        private const string USER_SETTINGS_TABLE_ID = "userSettings";

        //Identity Gateway values
        private const string role = "[\"admin\"]";
        private const string settingKey = "LastUsedTenant";

        // injected and created attribute
        private IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ILogger _log;
        private IIdentityGatewayClient _identityClient;

        //Helpers 

        private KeyVaultHelper keyVaultHelper;
        private TenantRunbookHelper tenantRunbookHelper;
        private CosmosHelper cosmosHelper;

        // collection and iothub naming 
        private string iotHubNameFormat = "iothub-{0}";  // format with a guid
        private string appConfigCollectionKeyFormat = "tenant:{0}:{1}-collection";  // format with a guid and collection name
        private List<string> tenantCollections = new List<string>{"telemetry", "twin-change", "lifecycle", "pcs"};

        public TenantController(IConfiguration config, IHttpContextAccessor httpContextAccessor, ILogger log, IIdentityGatewayClient identityGatewayClient)
        {
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
            this._log = log;

            this.keyVaultHelper = new KeyVaultHelper(this._config);
            this.tenantRunbookHelper = new TenantRunbookHelper(this._config);

            string cosmosDb = this._config[COSMOS_DB_KEY];
            string cosmosDbToken = this._config[COSMOS_KEY];
            this.cosmosHelper = new CosmosHelper(cosmosDb, cosmosDbToken);
            this._identityClient = identityGatewayClient;
        }

        // POST api/tenant
        [HttpPost]
        public async Task<string> PostAsync()
        {
            /* Creates a new tenant */
            // Load secrets from key vault
            var secretTasks = new Task<string>[] {
                this.keyVaultHelper.GetSecretAsync(STORAGE_ACCOUNT_CONNECTION_STRING_KEY),
                this.keyVaultHelper.GetSecretAsync(CREATE_IOTHUB_URL_KEY),
            };
            Task.WaitAll(secretTasks);
            string storageAccountConnectionString = secretTasks[0].Result;
            string createIotHubWebHookUrl = secretTasks[1].Result;

            // Generate new tenant information
            string tenantGuid = Guid.NewGuid().ToString();
            string iotHubName = String.Format(this.iotHubNameFormat, tenantGuid.Substring(0, 8));

            // Create a table storage helper now that we have the storage account conn string
            var tableStorageHelper = new TableStorageHelper(storageAccountConnectionString);

            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantGuid, iotHubName);
            await tableStorageHelper.WriteToTableAsync<TenantModel>(TENANT_TABLE_ID, tenant);

            // Trigger run book to create a new IoT Hub
            await this.tenantRunbookHelper.TriggerTenantRunbook(createIotHubWebHookUrl, tenantGuid, iotHubName);

            var userId = "";
            try
            {
                userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
                if (String.IsNullOrEmpty(userId))
                {
                    throw new NullReferenceException("The UserId retrieved from Http Context was null or empty.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve the userId from the httpContextAccessor.", e);
            }

            // Give the requesting user an admin role to the new tenant        

            try
            {
                await _identityClient.addUserToTenantAsync(userId, tenantGuid, role);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to add user to tenant.", e);
            }

            // Update the userSettings table with the lastUsedTenant if there isn't already a lastUsedTenant
            
            IdentityGatewayApiSettingModel userSettings = null;

            try
            {
                userSettings = await _identityClient.getSettingsForUserAsync(userId, settingKey);
            }
            catch(Exception e)
            {
                throw new Exception("Could not access user settings for LastUsedTenant.", e);
            }
            if (userSettings == null)
            {
                // Set the last used tenant to be this new tenant
                try
                {
                    await _identityClient.addSettingsForUserAsync(userId, settingKey, tenantGuid);
                }
                catch (Exception e)
                {
                    throw new Exception("Could not set user settings for LastUsedTenant.", e);
                }
            }

            // Write tenant info cosmos db collection name to app config
            try
            {
                var appConfigClient = new ConfigurationClient(this._config[APP_CONFIGURATION_KEY]);
                foreach (string collection in this.tenantCollections)
                {
                    string collectionKey = String.Format(this.appConfigCollectionKeyFormat, tenantGuid, collection);
                    string collectionId = $"{collection}-{tenantGuid}";
                    var collectionSetting = new ConfigurationSetting(collectionKey, collectionId);
                    try
                    {
                        appConfigClient.Set(collectionSetting);
                    }
                    catch (Exception e)
                    {
                        // log which key could not be created
                        throw new Exception($"Unable to create App Config key {collectionId}", e);
                    }
                }
            }
            catch (Exception e)
            {
                // In order for a complete tenant creation, all app config keys must be created. throw an error if not
                throw new Exception($"Unable to add required collection ids to App Config for tenant {tenantGuid}", e);
            }

            var response = new {
                message = "Your tenant is being created.",
                tenantId = tenantGuid
            };
            return JsonConvert.SerializeObject(response);
        }

        // GET api/tenant/<tenantId>
        [HttpGet("{tenantId}")]
        public async Task<TenantModel> GetAsync(string tenantId)
        {
            /* Returns information for a tenant */

            // Load variables from key vault
            var storageAccountConnectionString = await this.keyVaultHelper.GetSecretAsync(STORAGE_ACCOUNT_CONNECTION_STRING_KEY);

            var userId = "";
            TenantModel tenant = null;
            bool accessToTenant = false;
            try
            {
                userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
                if (String.IsNullOrEmpty(userId))
                {
                    throw new NullReferenceException($"The tenant {tenantId} does not exist in Table Storage.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve the userId from the httpContextAccessor", e);
            }
           
            // Create a table storage helper now that we have the storage account conn string
            var tableStorageHelper = new TableStorageHelper(storageAccountConnectionString);

            // Verify that the user has access to the specified tenant
            try
            {
                accessToTenant = await _identityClient.isUserAuthenticated(userId, tenantId);
            }
            catch (Exception e)
            {
                throw new NoAuthorizationException($"Unable to retrieve the user's tenant for User Id {userId}. The user may not be authorized.");
            }

            if (!accessToTenant)
            {
                throw new NoAuthorizationException($"Incorrect role for User Id {userId} associated with this tenant. The user may not be authorized.");
            }
            try
            {
                // Load the tenant from table storage
                tenant = await tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, tenantId.Substring(0, 1), tenantId);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve the tenant from table storage", e);
            }

            return tenant;
        }

         // DELETE api/tenantready/<tenantId>
        [HttpDelete("{tenantId}")]
        public async Task<string> DeleteAsync(string tenantId)
        {
            // Load secrets from key vault
            var secretTasks = new Task<string>[] {
                this.keyVaultHelper.GetSecretAsync(STORAGE_ACCOUNT_CONNECTION_STRING_KEY),
                this.keyVaultHelper.GetSecretAsync(DELETE_IOTHUB_URL_KEY)
            };
            Task.WaitAll(secretTasks);

            string storageAccountConnectionString = secretTasks[0].Result;
            string deleteIotHubWebHookUrl = secretTasks[1].Result;

            var userId = "";
            try
            {
                userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
                if (String.IsNullOrEmpty(userId))
                {
                    throw new NullReferenceException("The UserId retrieved from Http Context was null or empty.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve the userId from the httpContextAccessor", e);
            }

            Dictionary<string, bool> deletionRecord = new Dictionary<string, bool>{};
           
            // Create a table storage helper now that we have the storage account conn string
            var tableStorageHelper = new TableStorageHelper(storageAccountConnectionString);

            // Verify that the user has access to the specified tenant
            bool accessToTenant = false;
            try
            {
                accessToTenant = await _identityClient.isUserAuthenticated(userId, tenantId);
            }
            catch (Exception e)
            {
                throw new NoAuthorizationException($"Unable to retrieve the user's tenant for User Id {userId}. The user may not be authorized.");
            }

            if (!accessToTenant)
            {
                throw new NoAuthorizationException($"Incorrect role for User Id {userId} associated with this tenant. The user may not be authorized.");
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, partitionKey, tenantId);
            if (tenant == null)
            {
                this._log.Info($"The tenant {tenantId} could not be deleted from Table Storage because it does not exist.", () => new { tenantId });
                deletionRecord["tableStorage"] = true;
            }
            else
            {
                try
                {
                    await tableStorageHelper.DeleteEntityAsync<TenantModel>(TENANT_TABLE_ID, tenant);
                    deletionRecord["tableStorage"] = true;
                }
                catch (Exception e)
                {
                    string message = $"Unable to delete info from table storage for tenant {tenantId}";
                    this._log.Info(message, () => new { tenantId, e.Message });
                    deletionRecord["tableStorage"] = false;
                }
            }

            // Gather tenant information
            string tenantGuid = tenantId;
            string iotHubName = String.Format(this.iotHubNameFormat, tenantGuid.Substring(0, 8));

            try
            {
                //trigger delete iothub runbook
                await this.tenantRunbookHelper.TriggerTenantRunbook(deleteIotHubWebHookUrl, tenantGuid, iotHubName);
                deletionRecord["iotHub"] = true;
            }
            catch (Exception e)
            {
                string message = $"Unable to successfully trigger Delete IoTHub Runbook for tenant {tenantId}";
                this._log.Info(message, () => new { tenantId, e.Message });
                deletionRecord["iotHub"] = false;
            }

            // Delete collections
            string dbIdTms = this._config[DATABASE_KEY];
            string dbIdStorage = this._config[STORAGE_ADAPTER_DOCUMENT_DB_KEY];
            var appConfigClient = new ConfigurationClient(this._config[APP_CONFIGURATION_KEY]);
            foreach (string collection in this.tenantCollections)
            {
                // pcs colleciton uses a different database than the other collections
                string databaseId = collection == "pcs" ? dbIdStorage : dbIdTms;
                string collectionKey = String.Format(this.appConfigCollectionKeyFormat, tenantGuid, collection);
                string collectionId = "";
                try
                {
                    collectionId = appConfigClient.Get(collectionKey).Value.Value;
                }
                catch (Exception e)
                {
                    string message = $"Unable to retrieve the key {collectionKey} for a collection id in App Config for tenant {tenantGuid}";
                    this._log.Info(message, () => new { collectionKey, tenantGuid, e.Message });
                }

                if (String.IsNullOrEmpty(collectionId))
                {
                    string message = $"The collectionId was not set properly for tenant {tenantGuid} while attempting to delete the {collection} collection";
                    this._log.Info(message, () => new { collectionKey, tenantGuid });
                    // Currently, the assumption for an unknown collection id is that it has been deleted.
                    // We can come to this conclusion by assuming that the app config key containing the collection id was already deleted.
                    // TODO: Determine a more explicit outcome for this scenario - jrb
                    deletionRecord[$"{collection}Collection"] = true;
                    // If the collectionId could not be properly retrieved, go on to the next colleciton, do not attempt to delete.
                    continue;
                }

                try
                {
                    await cosmosHelper.DeleteCosmosDbCollection(databaseId, collectionId);
                    deletionRecord[$"{collection}Collection"] = true;
                }
                catch (ResourceNotFoundException e)
                {
                    string message = $"The {collection} collection for tenant {tenantGuid} does exist and cannot be deleted.";
                    this._log.Info(message, () => new { collectionId, tenantGuid, e.Message });
                    deletionRecord[$"{collection}Collection"] = true;
                }
                catch (Exception e)
                {
                    string message = $"An error occurred while deleting the {collection} collection for tenant {tenantGuid}";
                    deletionRecord[$"{collection}Collection"] = false;
                    this._log.Info(message, () => new { collectionId, tenantGuid, e.Message });
                }

                try
                {
                    // now that we have the collection Id, delete the key from app config
                    await appConfigClient.DeleteAsync(collectionKey);
                }
                catch (Exception e)
                {
                    string message = $"Unable to delete {collectionKey} from App Config";
                    this._log.Info(message, () => new { collectionKey, tenantGuid, e.Message });
                }
            }

            var response = new
            {
                tenantId = tenantGuid,
                fullyDeleted = deletionRecord.All(item => item.Value),
                deletionRecord = deletionRecord
            };
            return JsonConvert.SerializeObject(response);
        }
    }
}
