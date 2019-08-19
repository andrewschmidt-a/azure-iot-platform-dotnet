using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;
using IdentityGateway.Services.Models;
using IdentityGateway.Services.Helpers;

namespace IdentityGateway.Services
{
    public class UserSettingsContainer : UserContainer, IUserContainer<UserSettingsModel, UserSettingsInput>
    {
        public override string tableName { get{return "userSettings";} }

        public UserSettingsContainer(IHttpContextAccessor httpContextAccessor, TableHelper tableHelper) : base(httpContextAccessor, tableHelper)
        {
        }

        /// <summary>
        /// Get all settings for a given userId
        /// </summary>
        /// <param name="input">UserSettingsINput with a userId</param>
        /// <returns></returns>
        public async Task<List<UserSettingsModel>> GetAllAsync(UserSettingsInput input)
        {
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, input.userId));
            TableQuerySegment resultSegment = await this._tableHelper.QueryAsync(this.tableName, query, null);
            return (List<UserSettingsModel>)resultSegment.Results;
        }

        /// <summary>
        /// Get a specific user setting
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId and settingKey</param>
        /// <returns></returns>        
        public async Task<UserSettingsModel> GetAsync(UserSettingsInput input)
        {
            // TableOperation retrieveUserSettings = TableOperation.Retrieve<TableEntity>(input.userId, input.settingKey);
            TableOperation retrieveUserSettings = TableOperation.Retrieve<UserSettingsModel>(input.userId, input.settingKey);
            TableResult result = await this._tableHelper.ExecuteOperationAsync(this.tableName, retrieveUserSettings);
            return (UserSettingsModel)result.Result;
        }

        /// <summary>
        /// Create a new record in the user settings table
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId, settingkey name and value</param>
        /// <returns></returns>
        public async Task<UserSettingsModel> CreateAsync(UserSettingsInput input)
        {
            UserSettingsModel model = new UserSettingsModel(input);
            if (await this.GetAsync(input) != null)
            {
                // If this record already exists, return it without continuing with the insert operation
                return model;
            }
            TableOperation insertOperation = TableOperation.Insert(model);
            TableResult insert = await this._tableHelper.ExecuteOperationAsync(this.tableName, insertOperation);
            return (UserSettingsModel)insert.Result;
        }

        /// <summary>
        /// Update a record in the user settings table
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId, settingkey name and the udpated value</param>
        /// <returns></returns>
        public async Task<UserSettingsModel> UpdateAsync(UserSettingsInput input)
        {
            UserSettingsModel model = new UserSettingsModel(input);
            model.ETag = "*";
            TableOperation replaceOperation = TableOperation.Replace(model);
            TableResult replace = await this._tableHelper.ExecuteOperationAsync(this.tableName, replaceOperation);
            return (UserSettingsModel)replace.Result;
        }

        /// <summary>
        /// Delete a record from the user settings table
        /// </summary>
        /// <param name="input">UserSettingsInput with a userId and settingkey name</param>
        /// <returns></returns>
        public async Task<UserSettingsModel> DeleteAsync(UserSettingsInput input)
        {
            UserSettingsModel model = await this.GetAsync(input);
            TableOperation deleteOperation = TableOperation.Delete(model);
            TableResult delete = await this._tableHelper.ExecuteOperationAsync(this.tableName, deleteOperation);
            return (UserSettingsModel)delete.Result;
        }
    }
}