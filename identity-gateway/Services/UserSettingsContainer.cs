using System.Linq;
using System.Threading.Tasks;
using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace IdentityGateway.Services
{
    public class UserSettingsContainer : UserContainer, IUserContainer<UserSettingsModel, UserSettingsInput>
    {
        public override string TableName => "userSettings";

        public UserSettingsContainer() { }

        public UserSettingsContainer(ITableHelper tableHelper) : base(tableHelper)
        {
        }

        /// <summary>
        /// Get all settings for a given userId
        /// </summary>
        /// <param name="input">UserSettingsINput with a userId</param>
        /// <returns></returns>
        public virtual async Task<UserSettingsListModel> GetAllAsync(UserSettingsInput input)
        {
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, input.UserId));
            TableQuerySegment resultSegment = await this._tableHelper.QueryAsync(this.TableName, query, null);
            return new UserSettingsListModel("Get", resultSegment.Results.Select(t => (UserSettingsModel)t).ToList());
        }

        /// <summary>
        /// Get a specific user setting
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId and settingKey</param>
        /// <returns></returns>        
        public virtual async Task<UserSettingsModel> GetAsync(UserSettingsInput input)
        {
            // TableOperation retrieveUserSettings = TableOperation.Retrieve<TableEntity>(input.userId, input.settingKey);
            TableOperation retrieveUserSettings = TableOperation.Retrieve<UserSettingsModel>(input.UserId, input.SettingKey);
            TableResult result = await this._tableHelper.ExecuteOperationAsync(this.TableName, retrieveUserSettings);
            return (UserSettingsModel)result.Result;
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
                    " Use PUT instead to update this user instead."
                );
            }
            UserSettingsModel model = new UserSettingsModel(input);
            TableOperation insertOperation = TableOperation.Insert(model);
            TableResult insert = await this._tableHelper.ExecuteOperationAsync(this.TableName, insertOperation);
            return (UserSettingsModel)insert.Result;
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
            TableOperation replaceOperation = TableOperation.InsertOrReplace(model);
            TableResult replace = await this._tableHelper.ExecuteOperationAsync(this.TableName, replaceOperation);
            return (UserSettingsModel)replace.Result;
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
            TableOperation deleteOperation = TableOperation.Delete(model);
            TableResult delete = await this._tableHelper.ExecuteOperationAsync(this.TableName, deleteOperation);
            return (UserSettingsModel)delete.Result;
        }
    }
}