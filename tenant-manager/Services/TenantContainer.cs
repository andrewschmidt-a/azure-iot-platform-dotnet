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
        public readonly ILogger Logger;
        public readonly IIdentityGatewayClient IdentityClient;
        public readonly IDeviceGroupsConfigClient DeviceGroupClient;
        public readonly IRunbookHelper RunbookHelper;
        public readonly IStorageClient CosmosClient;
        public readonly ITableStorageClient TableStorageClient;
        public readonly IAppConfigurationHelper AppConfigHelper;
        private const string IotDatabaseId = "iot";
        private const string StorageAdapterDatabaseId = "pcs-storage";
        private const string TenantTableId = "tenant";
        private const string UserTableId = "user";
        private const string UserSettingsTableId = "userSettings";
        private const string LastUsedSettingKey = "LastUsedTenant";
        private const string CreatedRole = "[\"admin\"]";
        private string iotHubNameFormat = "iothub-{0}";  // format with a guid
        private string dpsNameFormat = "dps-{0}";  // format with a guid
        private string streamAnalyticsNameFormat = "sa-{0}";  // format with a guide
        private string appConfigCollectionKeyFormat = "tenant:{0}:{1}-collection";  // format with a guid and collection name
        private Dictionary<string, string> tenantCollections = new Dictionary<string, string>
        {
            { "telemetry", IotDatabaseId },
            { "twin-change", IotDatabaseId },
            { "lifecycle", IotDatabaseId },
            { "pcs", StorageAdapterDatabaseId }
        };

        public TenantContainer(
            IHttpContextAccessor httpContextAccessor,
            ILogger<TenantContainer> logger,
            IRunbookHelper RunbookHelper,
            IStorageClient cosmosClient,
            ITableStorageClient tableStorageClient,
            IIdentityGatewayClient identityGatewayClient,
            IDeviceGroupsConfigClient deviceGroupConfigClient,
            IAppConfigurationHelper appConfigHelper)
        {
            this.Logger = logger;
            this.RunbookHelper = RunbookHelper;
            this.CosmosClient = cosmosClient;
            this.TableStorageClient = tableStorageClient;
            this.IdentityClient = identityGatewayClient;
            this.DeviceGroupClient = deviceGroupConfigClient;
            this.AppConfigHelper = appConfigHelper;
        }

        public async Task<bool> TenantIsReadyAsync(string tenantId)
        {
            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await this.TableStorageClient.RetrieveAsync<TenantModel>(TenantTableId, partitionKey, tenantId);
            return tenant != null && tenant.IsIotHubDeployed;  // True if the tenant's IoTHub is fully deployed, false otherwise
        }

        public async Task<CreateTenantModel> CreateTenantAsync(string tenantId, string userId)
        {
            /* Creates a new tenant */
            string iotHubName = this.FormatResourceName(this.iotHubNameFormat, tenantId);
            string dpsName = this.FormatResourceName(this.dpsNameFormat, tenantId);

            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantId);
            await this.TableStorageClient.InsertAsync<TenantModel>(TenantTableId, tenant);

            // kick off provisioning runbooks
            await this.RunbookHelper.CreateIotHub(tenantId, iotHubName, dpsName);

            // Give the requesting user an admin role to the new tenant

            try
            {
                await IdentityClient.AddTenantForUserAsync(userId, tenantId, CreatedRole);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to add user to tenant.", e);
            }

            // Update the userSettings table with the lastUsedTenant if there isn't already a lastUsedTenant
            IdentityGatewayApiSettingModel userSettings = null;

            try
            {
                userSettings = await IdentityClient.GetSettingsForUserAsync(userId, LastUsedSettingKey);
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
                    await IdentityClient.AddSettingsForUserAsync(userId, LastUsedSettingKey, tenantId);
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
                    await this.AppConfigHelper.SetValueAsync(collectionKey, collectionId);
                }
            }
            catch (Exception e)
            {
                // In order for a complete tenant creation, all app config keys must be created. throw an error if not
                throw new Exception($"Unable to add required collection ids to App Config for tenant {tenantId}", e);
            }

            try
            {
                await this.DeviceGroupClient.CreateDefaultDeviceGroupAsync(tenantId);
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
                var tenant = await this.TableStorageClient.RetrieveAsync<TenantModel>(TenantTableId, tenantId.Substring(0, 1), tenantId);
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
            TenantModel tenant = await this.TableStorageClient.RetrieveAsync<TenantModel>(TenantTableId, partitionKey, tenantId);
            if (tenant != null && !tenant.IsIotHubDeployed && ensureFullyDeployed)
            {
                // If the tenant iothub is not deployed, we should not be able to start the delete process
                // this will mean the tenant is not fully deployed, so some resources could be deployed after
                // the delete process has begun
                throw new Exception("The tenant exists but it has not been fully deployed. Please wait for the tenant to fully deploy before trying to delete.");
            }
            else if (tenant == null)
            {
                Logger.LogInformation("The tenant {tenantId} could not be deleted from Table Storage because it does not exist or was not fully created.", tenantId);
                deletionRecord["tenantTableStorage"] = true;
            }
            else
            {
                try
                {
                    await this.TableStorageClient.DeleteAsync<TenantModel>(TenantTableId, tenant);
                    deletionRecord["tenantTableStorage"] = true;
                }
                catch (Exception e)
                {
                    Logger.LogInformation(e, "Unable to delete info from table storage for tenant {tenantId}", tenantId);
                    deletionRecord["tableStorage"] = false;
                }
            }

            // delete the tenant from the user
            try
            {
                await this.IdentityClient.DeleteTenantForAllUsersAsync(tenantId);
                deletionRecord["userTableStorage"] = true;
            }
            catch (Exception e)
            {
                Logger.LogInformation(e, "Unable to delete user-tenant relationships for tenant {tenantId} in the user table.", tenantId);
                deletionRecord["userTableStorage"] = false;
            }

            // update userSettings table LastUsedTenant if necessary
            try
            {
                IdentityGatewayApiSettingModel lastUsedTenant = await this.IdentityClient.GetSettingsForUserAsync(userId, "LastUsedTenant");
                if (lastUsedTenant.Value == tenantId)
                {
                    // update the LastUsedTenant to some null
                    await this.IdentityClient.UpdateSettingsForUserAsync(userId, "LastUsedTenant", string.Empty);
                }
            }
            catch (Exception e)
            {
                Logger.LogInformation(e, "Unable to get the user {userId} LastUsedTenant setting, the setting will not be updated.", userId);
            }

            // Gather tenant information
            string iotHubName = this.FormatResourceName(this.iotHubNameFormat, tenantId);
            string dpsName = this.FormatResourceName(this.dpsNameFormat, tenantId);
            // trigger delete iothub runbook
            try
            {
                await this.RunbookHelper.DeleteIotHub(tenantId, iotHubName, dpsName);
                deletionRecord["iotHub"] = true;
            }
            catch (Exception e)
            {
                Logger.LogInformation(e, "Unable to successfully trigger Delete IoTHub Runbook for tenant {tenantId}", tenantId);
                deletionRecord["iotHub"] = false;
            }

            string saJobName = this.FormatResourceName(this.streamAnalyticsNameFormat, tenantId);
            // trigger delete SA runbook
            try
            {
                await this.RunbookHelper.DeleteAlerting(tenantId, saJobName);
                deletionRecord["alerting"] = true;
            }
            catch (Exception e)
            {
                Logger.LogInformation(e, "Unable to successfully trigger Delete Alerting runbook for tenant {tenantId}", tenantId);
                deletionRecord["alerting"] = false;
            }

            // Delete collections
            foreach (KeyValuePair<string, string> collectionInfo in this.tenantCollections)
            {
                string collection = collectionInfo.Key;
                string databaseId = collectionInfo.Value;
                string collectionAppConfigKey = string.Format(this.appConfigCollectionKeyFormat, tenantId, collection);
                string collectionId = string.Empty;
                try
                {
                    collectionId = this.AppConfigHelper.GetValue(collectionAppConfigKey);
                }
                catch (Exception e)
                {
                    Logger.LogInformation(e, "Unable to retrieve the key {collectionKey} for a collection id in App Config for tenant {tenantId}", collectionAppConfigKey, tenantId);
                }

                if (string.IsNullOrEmpty(collectionId))
                {
                    Logger.LogInformation("The collectionId was not set properly for tenant {tenantId} while attempting to delete the {collection} collection", collectionAppConfigKey, tenantId);
                    // Currently, the assumption for an unknown collection id is that it has been deleted.
                    // We can come to this conclusion by assuming that the app config key containing the collection id was already deleted.
                    // TODO: Determine a more explicit outcome for this scenario - jrb
                    deletionRecord[$"{collection}Collection"] = true;
                    // If the collectionId could not be properly retrieved, go on to the next colleciton, do not attempt to delete.
                    continue;
                }

                try
                {
                    await this.CosmosClient.DeleteCollectionAsync(databaseId, collectionId);
                    deletionRecord[$"{collection}Collection"] = true;
                }
                catch (ResourceNotFoundException e)
                {
                    Logger.LogInformation(e, "The {collection} collection for tenant {tenantId} does exist and cannot be deleted.", collectionId, tenantId);
                    deletionRecord[$"{collection}Collection"] = true;
                }
                catch (Exception e)
                {
                    deletionRecord[$"{collection}Collection"] = false;
                    Logger.LogInformation(e, "An error occurred while deleting the {collection} collection for tenant {tenantId}", collectionId, tenantId);
                }

                try
                {
                    // now that we have the collection Id, delete the key from app config
                    await this.AppConfigHelper.DeleteKeyAsync(collectionAppConfigKey);
                }
                catch (Exception e)
                {
                    Logger.LogInformation(e, "Unable to delete {collectionKey} from App Config", collectionAppConfigKey, tenantId);
                }
            }

            return new DeleteTenantModel(tenantId, deletionRecord, ensureFullyDeployed);
        }

        private string FormatResourceName(string format, string tenantId)
        {
            return string.Format(format, tenantId.Substring(0, 8));
        }
    }
}