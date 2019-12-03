using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.AppConfiguration;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.TenantManager.Services.External;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;
using Mmm.Platform.IoT.TenantManager.Services.Models;
using Mmm.Platform.IoT.TenantManager.Services.Runtime;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public class TenantContainer : ITenantContainer
    {
        // table storage table ids
        private const string TENANT_TABLE_ID = "tenant";
        private const string USER_TABLE_ID = "user";
        private const string USER_SETTINGS_TABLE_ID = "userSettings";
        private const string LAST_USED_SETTING_KEY = "LastUsedTenant";
        private const string CREATED_ROLE = "[\"admin\"]";

        // collection and iothub naming 
        private string iotHubNameFormat = "iothub-{0}";  // format with a guid
        private string dpsNameFormat = "dps-{0}"; // format with a guid
        private string appConfigCollectionKeyFormat = "tenant:{0}:{1}-collection";  // format with a guid and collection name
        private List<string> tenantCollections = new List<string> { "telemetry", "twin-change", "lifecycle", "pcs" };

        public readonly IServicesConfig _config;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public readonly ILogger _log;
        public readonly IIdentityGatewayClient _identityClient;
        public readonly IDeviceGroupsConfigClient _deviceGroupClient;
        public readonly TenantRunbookHelper _tenantRunbookHelper;
        public readonly CosmosHelper _cosmosHelper;
        public readonly TableStorageHelper _tableStorageHelper;

        public TenantContainer(
            IServicesConfig config,
            IHttpContextAccessor httpContextAccessor,
            ILogger log,
            TenantRunbookHelper tenantRunbookHelper,
            CosmosHelper cosmosHelper,
            TableStorageHelper tableStorageHelper,
            IIdentityGatewayClient identityGatewayClient,
            IDeviceGroupsConfigClient deviceGroupConfigClient)
        {
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
            this._log = log;
            this._tenantRunbookHelper = tenantRunbookHelper;
            this._cosmosHelper = cosmosHelper;
            this._tableStorageHelper = tableStorageHelper;
            this._identityClient = identityGatewayClient;
            this._deviceGroupClient = deviceGroupConfigClient;
        }

        /// <summary>
        /// Check if the tenant has been fully deployed by ensuring that the tenant's IoT Hub is deployed.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns>bool</returns>
        public async Task<bool> TenantIsReadyAsync(string tenantId)
        {
            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await this._tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, partitionKey, tenantId);
            return (tenant != null && tenant.IsIotHubDeployed);  // True if the tenant's IoTHub is fully deployed, false otherwise
        }

        /// <summary>
        /// Create a new tenant and all required information realted to the tenant
        /// this process involves kicking off a long running Runbook webhook that provisions a new IoT Hub for the tenant
        /// Because of the long process, this method returns a response with the new tenant GUID and a message stating that the tenant is currently being created
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns>CreateTenantModel</returns>
        public async Task<CreateTenantModel> CreateTenantAsync(string tenantId)
        {
            /* Creates a new tenant */
            string iotHubName = String.Format(this.iotHubNameFormat, tenantId.Substring(0, 8));
            string dpsName = String.Format(this.dpsNameFormat, tenantId.Substring(0, 8));

            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantId, iotHubName);
            await this._tableStorageHelper.WriteToTableAsync<TenantModel>(TENANT_TABLE_ID, tenant);

            // Trigger run book to create a new IoT Hub
            await this._tenantRunbookHelper.CreateIotHub(tenantId, iotHubName, dpsName);

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
                await _identityClient.addTenantForUserAsync(userId, tenantId, CREATED_ROLE);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to add user to tenant.", e);
            }

            // Update the userSettings table with the lastUsedTenant if there isn't already a lastUsedTenant
            IdentityGatewayApiSettingModel userSettings = null;

            try
            {
                userSettings = await _identityClient.getSettingsForUserAsync(userId, LAST_USED_SETTING_KEY);
            }
            catch (Exception e)
            {
                throw new Exception("Could not access user settings for LastUsedTenant.", e);
            }
            if (userSettings == null)
            {
                // Set the last used tenant to be this new tenant
                try
                {
                    await _identityClient.addSettingsForUserAsync(userId, LAST_USED_SETTING_KEY, tenantId);
                }
                catch (Exception e)
                {
                    throw new Exception("Could not set user settings for LastUsedTenant.", e);
                }
            }

            // Write tenant info cosmos db collection name to app config
            try
            {
                var appConfigClient = new ConfigurationClient(this._config.ApplicationConfigurationConnectionString);
                foreach (string collection in this.tenantCollections)
                {
                    string collectionKey = String.Format(this.appConfigCollectionKeyFormat, tenantId, collection);
                    string collectionId = $"{collection}-{tenantId}";
                    var collectionSetting = new ConfigurationSetting(collectionKey, collectionId);
                    try
                    {
                        await appConfigClient.SetConfigurationSettingAsync(collectionSetting);
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
                throw new Exception($"Unable to add required collection ids to App Config for tenant {tenantId}", e);
            }

            try
            {
                await this._deviceGroupClient.CreateDefaultDeviceGroupAsync(tenantId);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create the default device group for the new tenant.", e);
            }

            return new CreateTenantModel(tenantId);
        }

        /// <summary>
        /// Get the TenantModel from table storage for the given tenantId
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns>TenantModel</returns>
        public async Task<TenantModel> GetTenantAsync(string tenantId)
        {
            try
            {
                // Load the tenant from table storage
                var tenant = await this._tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, tenantId.Substring(0, 1), tenantId);
                return tenant;
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve the tenant from table storage", e);
            }
        }

        /// <summary>
        /// /// Delete the tenant with the given tenantId
        /// This method keeps a record of all resources related to the tenant that have been deleted (or not)
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="ensureFullyDeployed">Optional parameter that will run a check for of whether or not the tenant is fully deployed before attempting to delete</param>
        /// <returns></returns>
        public async Task<DeleteTenantModel> DeleteTenantAsync(string tenantId, bool ensureFullyDeployed = true)
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

            Dictionary<string, bool> deletionRecord = new Dictionary<string, bool> { };

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await this._tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, partitionKey, tenantId);
            if (tenant != null && !tenant.IsIotHubDeployed && ensureFullyDeployed)
            {
                // If the tenant iothub is not deployed, we should not be able to start the delete process
                // this will mean the tenant is not fully deployed, so some resources could be deployed after
                // the delete process has begun
                throw new Exception("The tenant exists but it has not been fully deployed. Please wait for the tenant to fully deploy before trying to delete.");
            }
            else if (tenant == null)
            {
                this._log.Info($"The tenant {tenantId} could not be deleted from Table Storage because it does not exist or was not fully created.", () => new { tenantId });
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
                string dpsName = String.Format(this.dpsNameFormat, tenantId.Substring(0, 8));
                //trigger delete iothub runbook
                await this._tenantRunbookHelper.DeleteIotHub(tenantId, iotHubName, dpsName);
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
            var appConfigClient = new ConfigurationClient(this._config.ApplicationConfigurationConnectionString);
            foreach (string collection in this.tenantCollections)
            {
                // pcs colleciton uses a different database than the other collections
                string databaseId = collection == "pcs" ? dbIdStorage : dbIdTms;
                string collectionKey = String.Format(this.appConfigCollectionKeyFormat, tenantId, collection);
                string collectionId = "";
                try
                {
                    collectionId = (await appConfigClient.GetConfigurationSettingAsync(collectionKey)).Value.Value;
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
                    await appConfigClient.DeleteConfigurationSettingAsync(collectionKey);
                }
                catch (Exception e)
                {
                    string message = $"Unable to delete {collectionKey} from App Config";
                    this._log.Info(message, () => new { collectionKey, tenantId, e.Message });
                }
            }

            return new DeleteTenantModel(tenantId, deletionRecord, ensureFullyDeployed);
        }
    }
}