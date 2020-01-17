using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Microsoft.Azure.Cosmos.Table;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public class UserSettingsContainer : UserContainer, IUserContainer<UserSettingsModel, UserSettingsInput>
    {
        public override string TableName => "userSettings";

        public UserSettingsContainer() { }

        public UserSettingsContainer(ITableStorageClient tableStorageClient) : base(tableStorageClient)
        {
        }

        /// <summary>
        /// Get all settings for a given userId
        /// </summary>
        /// <param name="input">UserSettingsINput with a userId</param>
        /// <returns></returns>
        public virtual async Task<UserSettingsListModel> GetAllAsync(UserSettingsInput input)
        {
            TableQuery<UserSettingsModel> query = new TableQuery<UserSettingsModel>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, input.UserId));
            List<UserSettingsModel> result = await this._tableStorageClient.QueryAsync<UserSettingsModel>(this.TableName, query);
            return new UserSettingsListModel("Get", result);
        }

        /// <summary>
        /// Get a specific user setting
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId and settingKey</param>
        /// <returns></returns>        
        public virtual async Task<UserSettingsModel> GetAsync(UserSettingsInput input)
        {
            return await this._tableStorageClient.RetrieveAsync<UserSettingsModel>(this.TableName, input.UserId, input.SettingKey);
        }

        /// <summary>
        /// Create a new record in the user settings table
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId, settingkey name and value</param>
        /// <returns></returns>
        public virtual async Task<UserSettingsModel> CreateAsync(UserSettingsInput input)
        {
            UserSettingsModel existingModel = await this.GetAsync(input);
            if (existingModel != null)
            {
                // If this record already exists, return it without continuing with the insert operation
                throw new StorageException
                (
                    $"That UserSetting already exists with value {existingModel.Value}." +
                    " Use PUT to update this user instead.");
            }
            UserSettingsModel model = new UserSettingsModel(input);
            return await this._tableStorageClient.InsertAsync(this.TableName, model);
        }

        /// <summary>
        /// Update a record in the user settings table
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId, settingkey name and the udpated value</param>
        /// <returns></returns>
        public virtual async Task<UserSettingsModel> UpdateAsync(UserSettingsInput input)
        {
            UserSettingsModel model = new UserSettingsModel(input);
            model.ETag = "*";  // An ETag is required for updating - this allows any etag to be used
            return await this._tableStorageClient.InsertOrReplaceAsync<UserSettingsModel>(this.TableName, model);
        }

        /// <summary>
        /// Delete a record from the user settings table
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId and settingkey name</param>
        /// <returns></returns>
        public virtual async Task<UserSettingsModel> DeleteAsync(UserSettingsInput input)
        {
            UserSettingsModel model = await this.GetAsync(input);
            if (model == null)
            {
                throw new StorageException($"That UserSetting does not exist");
            }

            model.ETag = "*";  // An ETag is required for deleting - this allows any etag to be used
            return await this._tableStorageClient.DeleteAsync<UserSettingsModel>(this.TableName, model);
        }
    }
}