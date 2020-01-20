using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.TenantManager.Services.External;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public class TenantContainer : ITenantContainer
    {
        // database ids for various collecitons
        private const string IOT_DATABASE_ID = "iot";
        private const string STORAGE_ADAPTER_DATABASE_ID = "pcs-storage";

        // table storage table ids
        private const string TENANT_TABLE_ID = "tenant";
        private const string USER_TABLE_ID = "user";
        private const string USER_SETTINGS_TABLE_ID = "userSettings";
        private const string LAST_USED_SETTING_KEY = "LastUsedTenant";
        private const string CREATED_ROLE = "[\"admin\"]";

        // collection and iothub naming
        private string iotHubNameFormat = "iothub-{0}";  // format with a guid
        private string dpsNameFormat = "dps-{0}";  // format with a guid
        private string streamAnalyticsNameFormat = "sa-{0}";  // format with a guide
        private string appConfigCollectionKeyFormat = "tenant:{0}:{1}-collection";  // format with a guid and collection name
        private Dictionary<string, string> tenantCollections = new Dictionary<string, string>
        {
            { "telemetry", IOT_DATABASE_ID },
            { "twin-change", IOT_DATABASE_ID },
            { "lifecycle", IOT_DATABASE_ID },
            { "pcs", STORAGE_ADAPTER_DATABASE_ID }
        };

        public readonly ILogger _logger;
        public readonly IIdentityGatewayClient _identityClient;
        public readonly IDeviceGroupsConfigClient _deviceGroupClient;
        public readonly IRunbookHelper _runbookHelper;
        public readonly IStorageClient _cosmosClient;
        public readonly ITableStorageClient _tableStorageClient;
        public readonly IAppConfigurationHelper _appConfigHelper;

        public TenantContainer(
            IHttpContextAccessor httpContextAccessor,
            ILogger<TenantContainer> log,
            IRunbookHelper RunbookHelper,
            IStorageClient cosmosClient,
            ITableStorageClient tableStorageClient,
            IIdentityGatewayClient identityGatewayClient,
            IDeviceGroupsConfigClient deviceGroupConfigClient,
            IAppConfigurationHelper appConfigHelper)
        {
            _logger = log;
            this._runbookHelper = RunbookHelper;
            this._cosmosClient = cosmosClient;
            this._tableStorageClient = tableStorageClient;
            this._identityClient = identityGatewayClient;
            this._deviceGroupClient = deviceGroupConfigClient;
            this._appConfigHelper = appConfigHelper;
        }

        private string FormatResourceName(string format, string tenantId)
        {
            return string.Format(format, tenantId.Substring(0, 8));
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
            TenantModel tenant = await this._tableStorageClient.RetrieveAsync<TenantModel>(TENANT_TABLE_ID, partitionKey, tenantId);
            return tenant != null && tenant.IsIotHubDeployed;  // True if the tenant's IoTHub is fully deployed, false otherwise
        }

        /// <summary>
        /// Create a new tenant and all required information realted to the tenant
        /// this process involves kicking off a long running Runbook webhook that provisions a new IoT Hub for the tenant
        /// Because of the long process, this method returns a response with the new tenant GUID and a message stating that the tenant is currently being created
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns>CreateTenantModel</returns>
        public async Task<CreateTenantModel> CreateTenantAsync(string tenantId, string userId)
        {
            /* Creates a new tenant */
            string iotHubName = this.FormatResourceName(this.iotHubNameFormat, tenantId);
            string dpsName = this.FormatResourceName(this.dpsNameFormat, tenantId);

            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantId);
            await this._tableStorageClient.InsertAsync<TenantModel>(TENANT_TABLE_ID, tenant);

            // kick off provisioning runbooks
            await this._runbookHelper.CreateIotHub(tenantId, iotHubName, dpsName);

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
                foreach (string collection in this.tenantCollections.Keys)
                {
                    string collectionKey = string.Format(this.appConfigCollectionKeyFormat, tenantId, collection);
                    string collectionId = $"{collection}-{tenantId}";
                    await this._appConfigHelper.SetValueAsync(collectionKey, collectionId);
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
                var tenant = await this._tableStorageClient.RetrieveAsync<TenantModel>(TENANT_TABLE_ID, tenantId.Substring(0, 1), tenantId);
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
        public async Task<DeleteTenantModel> DeleteTenantAsync(string tenantId, string userId, bool ensureFullyDeployed = true)
        {
            Dictionary<string, bool> deletionRecord = new Dictionary<string, bool> { };

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await this._tableStorageClient.RetrieveAsync<TenantModel>(TENANT_TABLE_ID, partitionKey, tenantId);
            if (tenant != null && !tenant.IsIotHubDeployed && ensureFullyDeployed)
            {
                // If the tenant iothub is not deployed, we should not be able to start the delete process
                // this will mean the tenant is not fully deployed, so some resources could be deployed after
                // the delete process has begun
                throw new Exception("The tenant exists but it has not been fully deployed. Please wait for the tenant to fully deploy before trying to delete.");
            }
            else if (tenant == null)
            {
                _logger.LogInformation("The tenant {tenantId} could not be deleted from Table Storage because it does not exist or was not fully created.", tenantId);
                deletionRecord["tenantTableStorage"] = true;
            }
            else
            {
                try
                {
                    await this._tableStorageClient.DeleteAsync<TenantModel>(TENANT_TABLE_ID, tenant);
                    deletionRecord["tenantTableStorage"] = true;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Unable to delete info from table storage for tenant {tenantId}", tenantId);
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
                _logger.LogInformation(e, "Unable to delete user-tenant relationships for tenant {tenantId} in the user table.", tenantId);
                deletionRecord["userTableStorage"] = false;
            }

            // update userSettings table LastUsedTenant if necessary
            try
            {
                IdentityGatewayApiSettingModel lastUsedTenant = await this._identityClient.getSettingsForUserAsync(userId, "LastUsedTenant");
                if (lastUsedTenant.Value == tenantId)
                {
                    // update the LastUsedTenant to some null
                    await this._identityClient.updateSettingsForUserAsync(userId, "LastUsedTenant", "");
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Unable to get the user {userId} LastUsedTenant setting, the setting will not be updated.", userId);
            }

            // Gather tenant information
            string iotHubName = this.FormatResourceName(this.iotHubNameFormat, tenantId);
            string dpsName = this.FormatResourceName(this.dpsNameFormat, tenantId);
            // trigger delete iothub runbook
            try
            {
                await this._runbookHelper.DeleteIotHub(tenantId, iotHubName, dpsName);
                deletionRecord["iotHub"] = true;
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Unable to successfully trigger Delete IoTHub Runbook for tenant {tenantId}", tenantId);
                deletionRecord["iotHub"] = false;
            }

            string saJobName = this.FormatResourceName(this.streamAnalyticsNameFormat, tenantId);
            // trigger delete SA runbook
            try
            {
                await this._runbookHelper.DeleteAlerting(tenantId, saJobName);
                deletionRecord["alerting"] = true;
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Unable to successfully trigger Delete Alerting runbook for tenant {tenantId}", tenantId);
                deletionRecord["alerting"] = false;
            }

            // Delete collections
            foreach (KeyValuePair<string, string> collectionInfo in this.tenantCollections)
            {
                string collection = collectionInfo.Key;
                string databaseId = collectionInfo.Value;
                string collectionAppConfigKey = string.Format(this.appConfigCollectionKeyFormat, tenantId, collection);
                string collectionId = "";
                try
                {
                    collectionId = this._appConfigHelper.GetValue(collectionAppConfigKey);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Unable to retrieve the key {collectionKey} for a collection id in App Config for tenant {tenantId}", collectionAppConfigKey, tenantId);
                }

                if (string.IsNullOrEmpty(collectionId))
                {
                    _logger.LogInformation("The collectionId was not set properly for tenant {tenantId} while attempting to delete the {collection} collection", collectionAppConfigKey, tenantId);
                    // Currently, the assumption for an unknown collection id is that it has been deleted.
                    // We can come to this conclusion by assuming that the app config key containing the collection id was already deleted.
                    // TODO: Determine a more explicit outcome for this scenario - jrb
                    deletionRecord[$"{collection}Collection"] = true;
                    // If the collectionId could not be properly retrieved, go on to the next colleciton, do not attempt to delete.
                    continue;
                }

                try
                {
                    await this._cosmosClient.DeleteCollectionAsync(databaseId, collectionId);
                    deletionRecord[$"{collection}Collection"] = true;
                }
                catch (ResourceNotFoundException e)
                {
                    _logger.LogInformation(e, "The {collection} collection for tenant {tenantId} does exist and cannot be deleted.", collectionId, tenantId);
                    deletionRecord[$"{collection}Collection"] = true;
                }
                catch (Exception e)
                {
                    deletionRecord[$"{collection}Collection"] = false;
                    _logger.LogInformation(e, "An error occurred while deleting the {collection} collection for tenant {tenantId}", collectionId, tenantId);
                }

                try
                {
                    // now that we have the collection Id, delete the key from app config
                    await this._appConfigHelper.DeleteKeyAsync(collectionAppConfigKey);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Unable to delete {collectionKey} from App Config", collectionAppConfigKey, tenantId);
                }
            }

            return new DeleteTenantModel(tenantId, deletionRecord, ensureFullyDeployed);
        }
    }
}