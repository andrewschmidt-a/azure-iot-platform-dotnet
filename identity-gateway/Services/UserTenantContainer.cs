using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public class UserTenantContainer : UserContainer, IUserContainer<UserTenantModel, UserTenantInput>
    {
        public UserTenantContainer()
        {
        }

        public UserTenantContainer(ITableStorageClient tableStorageClient) : base(tableStorageClient)
        {
        }

        public override string TableName => "user";

        /// <summary>
        /// get all tenants for a user
        /// </summary>
        /// <param name="input">UserTenantInput with the userId param</param>
        /// <returns></returns>
        public virtual async Task<UserTenantListModel> GetAllAsync(UserTenantInput input)
        {
            TableQuery<UserTenantModel> query = new TableQuery<UserTenantModel>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, input.UserId));
            List<UserTenantModel> result = await this._tableStorageClient.QueryAsync<UserTenantModel>(this.TableName, query);
            return new UserTenantListModel("GetTenants", result);
        }


        /// <summary>
        /// get all users for a tenant
        /// </summary>
        /// <param name="input">UserTenantInput with the tenant param</param>
        /// <returns></returns>
        public virtual async Task<UserTenantListModel> GetAllUsersAsync(UserTenantInput input)
        {
            TableQuery<UserTenantModel> query = new TableQuery<UserTenantModel>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, input.Tenant));
            List<UserTenantModel> result = await this._tableStorageClient.QueryAsync<UserTenantModel>(this.TableName, query);
            return new UserTenantListModel("GetUsers", result);
        }
        /// <summary>
        /// Get a single tenant for the user
        /// </summary>
        /// <param name="input">UserTenantInput with a userid</param>
        /// <returns></returns>
        public virtual async Task<UserTenantModel> GetAsync(UserTenantInput input)
        {
            return await this._tableStorageClient.RetrieveAsync<UserTenantModel>(this.TableName, input.UserId, input.Tenant);
        }

        /// <summary>
        /// Create a User record in the UserTenantContainer using the given userId and current tenant
        /// </summary>
        /// <param name="input">UserTenantInput with a userId</param>
        /// <returns></returns>
        public virtual async Task<UserTenantModel> CreateAsync(UserTenantInput input)
        {
            // In order to create a new user with a tenant, create a new user id
            if (input.UserId == null)
            {
                input.UserId = Guid.NewGuid().ToString();
            }
            // Create the user and options for creating the user record in the user table
            UserTenantModel existingModel = await this.GetAsync(input);
            if (existingModel != null)
            {
                // If this record already exists, return it without continuing with the insert operation
                throw new StorageException
                (
                    $"That UserTenant record already exists with value {existingModel.Roles}." +
                    " Use PUT instead to update this setting instead.");
            }
            UserTenantModel user = new UserTenantModel(input);
            return await this._tableStorageClient.InsertAsync(this.TableName, user);
        }

        /// <summary>
        /// Update a user record
        /// </summary>
        /// <param name="input">UserTenantInput with a userId, tenant, and rolelist</param>
        /// <returns></returns>
        public virtual async Task<UserTenantModel> UpdateAsync(UserTenantInput input)
        {
            UserTenantModel model = new UserTenantModel(input);
            if (model.RoleList != null && !model.RoleList.Any())
            {
                // If the RoleList of the model is empty, throw an exception. The RoleList is the only updateable feature of the UserTenant Table
                throw new ArgumentException("The UserTenant update model must contain a serialized role array.");
            }
            model.ETag = "*";  // An ETag is required for updating - this allows any etag to be used
            return await this._tableStorageClient.InsertOrMergeAsync(this.TableName, model);
        }

        /// <summary>
        /// Delete a User record in the UserTenantContainer using the given userId and current tenant
        /// </summary>
        /// <param name="input">UserTenantInput with a userId</param>
        /// <returns></returns>
        public virtual async Task<UserTenantModel> DeleteAsync(UserTenantInput input)
        {
            // Get a list of all user models for this user id - we will pick the one matching the current tenant to delete
            UserTenantModel user = await this.GetAsync(input);
            if (user == null)
            {
                throw new StorageException($"That UserTenant does not exist");
            }

            user.ETag = "*";  // An ETag is required for deleting - this allows any etag to be used
            return await this._tableStorageClient.DeleteAsync(this.TableName, user);
        }

        /// <summary>
        /// Delete the tenant from all users
        /// </summary>
        /// <returns></returns>
        public virtual async Task<UserTenantListModel> DeleteAllAsync(UserTenantInput input)
        {
            UserTenantListModel tenantRows = await this.GetAllUsersAsync(input);

            // delete all rows as one asynchronous job
            var deleteTasks = tenantRows.models.Select(row =>
            {
                UserTenantInput deleteInput = new UserTenantInput
                {
                    UserId = row.PartitionKey,
                    Tenant = input.Tenant
                };
                return this.DeleteAsync(deleteInput);
            });
            var deletionResult = await Task.WhenAll(deleteTasks);
            return new UserTenantListModel("Delete", deletionResult.ToList());
        }
    }
}