// <copyright file="UserSettingsContainer.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Mmm.Iot.Common.Services.External.TableStorage;
using Mmm.Iot.IdentityGateway.Services.Models;

namespace Mmm.Iot.IdentityGateway.Services
{
    public class UserSettingsContainer : UserContainer, IUserContainer<UserSettingsModel, UserSettingsInput>
    {
        public UserSettingsContainer()
        {
        }

        public UserSettingsContainer(ITableStorageClient tableStorageClient)
            : base(tableStorageClient)
        {
        }

        public override string TableName => "userSettings";

        public virtual async Task<UserSettingsListModel> GetAllAsync(UserSettingsInput input)
        {
            TableQuery<UserSettingsModel> query = new TableQuery<UserSettingsModel>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, input.UserId));
            List<UserSettingsModel> result = await this.TableStorageClient.QueryAsync<UserSettingsModel>(this.TableName, query);
            return new UserSettingsListModel("Get", result);
        }

        public virtual async Task<UserSettingsModel> GetAsync(UserSettingsInput input)
        {
            return await this.TableStorageClient.RetrieveAsync<UserSettingsModel>(this.TableName, input.UserId, input.SettingKey);
        }

        public virtual async Task<UserSettingsModel> CreateAsync(UserSettingsInput input)
        {
            UserSettingsModel existingModel = await this.GetAsync(input);
            if (existingModel != null)
            {
                // If this record already exists, return it without continuing with the insert operation
                throw new StorageException(
                    $"That UserSetting already exists with value {existingModel.Value}." +
                    " Use PUT to update this user instead.");
            }

            UserSettingsModel model = new UserSettingsModel(input);
            return await this.TableStorageClient.InsertAsync(this.TableName, model);
        }

        public virtual async Task<UserSettingsModel> UpdateAsync(UserSettingsInput input)
        {
            UserSettingsModel model = new UserSettingsModel(input);
            model.ETag = "*";  // An ETag is required for updating - this allows any etag to be used
            return await this.TableStorageClient.InsertOrReplaceAsync<UserSettingsModel>(this.TableName, model);
        }

        public virtual async Task<UserSettingsModel> DeleteAsync(UserSettingsInput input)
        {
            UserSettingsModel model = await this.GetAsync(input);
            if (model == null)
            {
                throw new StorageException($"That UserSetting does not exist");
            }

            model.ETag = "*";  // An ETag is required for deleting - this allows any etag to be used
            return await this.TableStorageClient.DeleteAsync<UserSettingsModel>(this.TableName, model);
        }
    }
}