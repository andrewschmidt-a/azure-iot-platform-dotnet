using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.Services.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.Services.External;
using MMM.Azure.IoTSolutions.TenantManager.Services.Exceptions;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;
using Azure.ApplicationModel.Configuration;
using ILogger = MMM.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class TenantController : ControllerBase
    {
        // table storage table ids
        private const string TENANT_TABLE_ID = "tenant";
        private const string USER_TABLE_ID = "user";
        private const string USER_SETTINGS_TABLE_ID = "userSettings";

        //Identity Gateway values
        private const string role = "[\"admin\"]";
        private const string settingKey = "LastUsedTenant";

        // injected and created attribute
        private IServicesConfig _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ILogger _log;
        private IIdentityGatewayClient _identityClient;

        //Helpers 
        private TenantRunbookHelper _tenantRunbookHelper;
        private CosmosHelper _cosmosHelper;
        private TableStorageHelper _tableStorageHelper;

        // collection and iothub naming 
        private string iotHubNameFormat = "iothub-{0}";  // format with a guid
        private string appConfigCollectionKeyFormat = "tenant:{0}:{1}-collection";  // format with a guid and collection name
        private List<string> tenantCollections = new List<string>{"telemetry", "twin-change", "lifecycle", "pcs"};

        public TenantController(
            IServicesConfig config,
            IHttpContextAccessor httpContextAccessor,
            ILogger log,
            TenantRunbookHelper tenantRunbookHelper,
            CosmosHelper cosmosHelper,
            TableStorageHelper tableStorageHelper,
            IIdentityGatewayClient identityGatewayClient)
        {
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
            this._log = log;
            this._tenantRunbookHelper = tenantRunbookHelper;
            this._cosmosHelper = cosmosHelper;
            this._tableStorageHelper = tableStorageHelper;
            this._identityClient = identityGatewayClient;
        }

        // POST api/tenant
        [HttpPost]
        public async Task<string> PostAsync()
        {
            /* Creates a new tenant */
            // Generate new tenant information
            string tenantGuid = Guid.NewGuid().ToString();
            string iotHubName = String.Format(this.iotHubNameFormat, tenantGuid.Substring(0, 8));

            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantGuid, iotHubName);
            await this._tableStorageHelper.WriteToTableAsync<TenantModel>(TENANT_TABLE_ID, tenant);

            // Trigger run book to create a new IoT Hub
            await this._tenantRunbookHelper.CreateIotHub(tenantGuid, iotHubName);

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
                await _identityClient.addTenantForUserAsync(userId, tenantGuid, role);
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
                var appConfigClient = new ConfigurationClient(this._config.AppConfigEndpoint);
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

            // Verify that the user has access to the specified tenant
            try
            {
                accessToTenant = await this._identityClient.isUserAuthenticated(userId, tenantId);
                if (!accessToTenant)
                {
                    throw new NoAuthorizationException($"The User {userId} is not authorized for operations on this tenant. The user may not have the proper role for this operation.");
                }
            }
            catch (Exception e)
            {
                throw new NoAuthorizationException($"The User {userId} is not authorized for operations on this tenant.", e);
            }
            try
            {
                // Load the tenant from table storage
                tenant = await this._tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, tenantId.Substring(0, 1), tenantId);
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

            // Verify that the user has access to the specified tenant
            try
            {
                bool accessToTenant = await this._identityClient.isUserAuthenticated(userId, tenantId);
                if (!accessToTenant)
                {
                    throw new NoAuthorizationException($"Incorrect role for User Id {userId} associated with this tenant. The user may not be authorized. The user may not have the proper role for this operation.");
                }
            }
            catch (Exception e)
            {
                throw new NoAuthorizationException($"The User {userId} is not authorized for operations on this tenant.", e);
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await this._tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, partitionKey, tenantId);
            if (tenant != null && !tenant.IsIotHubDeployed)
            {
                // If the tenant iothub is not deployed, we should not be able to start the delete process
                // this will mean the tenant is not fully deployed, so some resources could be deployed after
                // the delete process has begun
                throw new Exception("The tenant exists but it has not been fully deployed. Please wait for the tenant to fully deploy before trying to delete.");
            }
            else if (tenant == null)
            {
                this._log.Info($"The tenant {tenantId} could not be deleted from Table Storage because it does not exist.", () => new { tenantId });
                deletionRecord["tenantTableStorage"] = true;
            }
            else
            {
                try
                {
                    await this._tableStorageHelper.DeleteEntityAsync<TenantModel>(TENANT_TABLE_ID, tenant);
                    deletionRecord["tenantTableStorage"] = true;
                }
                catch (Exception e)
                {
                    string message = $"Unable to delete info from table storage for tenant {tenantId}";
                    this._log.Info(message, () => new { tenantId, e.Message });
                    deletionRecord["tableStorage"] = false;
                }
            }

            // delete the tenant from the user
            try
            {
                await this._identityClient.deleteTenantForAllUsersAsync(tenantId);
                deletionRecord["userTableStorage"] = true;
            }
            catch (Exception e)
            {
                this._log.Info($"Unable to delete user-tenant relationships for tenant {tenantId} in the user table.", () => new { tenantId, e.Message });
                deletionRecord["userTableStorage"] = false;
            }

            // update userSettings table LastUsedTenant if necessary
            try
            {
                IdentityGatewayApiSettingModel lastUsedTenant = await this._identityClient.getSettingsForUserAsync(userId, "LastUsedTenant");
                if (lastUsedTenant.Value == tenantId)  // Value is the tenantId in the model
                {
                    // update the LastUsedTenant to some null
                    await this._identityClient.updateSettingsForUserAsync(userId, "LastUsedTenant", "");
                }
            }
            catch (Exception e)
            {
                this._log.Info($"Unable to get the user {userId} LastUsedTenant setting, the setting will not be updated.", () => new { userId, e.Message });
            }

            // Gather tenant information
            try
            {
                string iotHubName = String.Format(this.iotHubNameFormat, tenantId.Substring(0, 8));
                //trigger delete iothub runbook
                await this._tenantRunbookHelper.DeleteIotHub(tenantId, iotHubName);
                deletionRecord["iotHub"] = true;
            }
            catch (Exception e)
            {
                string message = $"Unable to successfully trigger Delete IoTHub Runbook for tenant {tenantId}";
                this._log.Info(message, () => new { tenantId, e.Message });
                deletionRecord["iotHub"] = false;
            }

            // Delete collections
            string dbIdTms = this._config.TenantManagerDatabaseId;
            string dbIdStorage = this._config.StorageAdapterDatabseId;
            var appConfigClient = new ConfigurationClient(this._config.AppConfigConnectionString);
            foreach (string collection in this.tenantCollections)
            {
                // pcs colleciton uses a different database than the other collections
                string databaseId = collection == "pcs" ? dbIdStorage : dbIdTms;
                string collectionKey = String.Format(this.appConfigCollectionKeyFormat, tenantId, collection);
                string collectionId = "";
                try
                {
                    collectionId = appConfigClient.Get(collectionKey).Value.Value;
                }
                catch (Exception e)
                {
                    string message = $"Unable to retrieve the key {collectionKey} for a collection id in App Config for tenant {tenantId}";
                    this._log.Info(message, () => new { collectionKey, tenantId, e.Message });
                }

                if (String.IsNullOrEmpty(collectionId))
                {
                    string message = $"The collectionId was not set properly for tenant {tenantId} while attempting to delete the {collection} collection";
                    this._log.Info(message, () => new { collectionKey, tenantId });
                    // Currently, the assumption for an unknown collection id is that it has been deleted.
                    // We can come to this conclusion by assuming that the app config key containing the collection id was already deleted.
                    // TODO: Determine a more explicit outcome for this scenario - jrb
                    deletionRecord[$"{collection}Collection"] = true;
                    // If the collectionId could not be properly retrieved, go on to the next colleciton, do not attempt to delete.
                    continue;
                }

                try
                {
                    await this._cosmosHelper.DeleteCosmosDbCollection(databaseId, collectionId);
                    deletionRecord[$"{collection}Collection"] = true;
                }
                catch (ResourceNotFoundException e)
                {
                    string message = $"The {collection} collection for tenant {tenantId} does exist and cannot be deleted.";
                    this._log.Info(message, () => new { collectionId, tenantId, e.Message });
                    deletionRecord[$"{collection}Collection"] = true;
                }
                catch (Exception e)
                {
                    string message = $"An error occurred while deleting the {collection} collection for tenant {tenantId}";
                    deletionRecord[$"{collection}Collection"] = false;
                    this._log.Info(message, () => new { collectionId, tenantId, e.Message });
                }

                try
                {
                    // now that we have the collection Id, delete the key from app config
                    await appConfigClient.DeleteAsync(collectionKey);
                }
                catch (Exception e)
                {
                    string message = $"Unable to delete {collectionKey} from App Config";
                    this._log.Info(message, () => new { collectionKey, tenantId, e.Message });
                }
            }

            var response = new
            {
                tenantId = tenantId,
                fullyDeleted = deletionRecord.All(item => item.Value),
                deletionRecord = deletionRecord
            };
            return JsonConvert.SerializeObject(response);
        }
    }
}
