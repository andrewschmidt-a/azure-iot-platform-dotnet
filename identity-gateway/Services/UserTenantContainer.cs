using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using IdentityGateway.Services.Models;
using IdentityGateway.Services.Helpers;

namespace IdentityGateway.Services
{
    public class UserTenantContainer : UserContainer, IUserContainer<UserTenantModel, UserTenantInput> 
    {
        public override string tableName { get{return "user";} }

        public UserTenantContainer(TableHelper tableHelper) : base(tableHelper)
        {
        }

        /// <summary>
        /// get all tenants for a user
        /// </summary>
        /// <param name="input">UserTenantInput with the userId param</param>
        /// <returns></returns>
        public async Task<UserTenantListModel> GetAllAsync(UserTenantInput input)
        {
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, input.userId));
            TableQuerySegment resultSegment = await this._tableHelper.QueryAsync(this.tableName, query, null);
            return new UserTenantListModel(resultSegment.Results.Select(t => (UserTenantModel)t).ToList());  // cast to a UserTenantModel list to easily parse result
        }


        /// <summary>
        /// get all users for a tenant
        /// </summary>
        /// <param name="input">UserTenantInput with the tenant param</param>
        /// <returns></returns>
        public async Task<UserTenantListModel> GetAllUsersAsync(UserTenantInput input)
        {
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, input.tenant));
            TableQuerySegment resultSegment = await this._tableHelper.QueryAsync(this.tableName, query, null);
            return new UserTenantListModel(resultSegment.Results.Select(t => (UserTenantModel)t).ToList());  // cast to a UserTenantModel list to easily parse result
        }
        /// <summary>
        /// Get a single tenant for the user
        /// </summary>
        /// <param name="input">UserTenantInput with a userid</param>
        /// <returns></returns>
        public async Task<UserTenantModel> GetAsync(UserTenantInput input)
        {
            TableOperation retrieveUserTenant = TableOperation.Retrieve<UserTenantModel>(input.userId, input.tenant);
            TableResult result = await this._tableHelper.ExecuteOperationAsync(this.tableName, retrieveUserTenant);
            return result.Result as UserTenantModel;
        }

        /// <summary>
        /// Create a User record in the UserTenantContainer using the given userId and current tenant
        /// </summary>
        /// <param name="input">UserTenantInput with a userId</param>
        /// <returns></returns>
        public async Task<UserTenantModel> CreateAsync(UserTenantInput input)
        {
            // If UserId is null then make it up
            if (input.userId == null)
            {
                input.userId = Guid.NewGuid().ToString();
            }
            // Create the user and options for creating the user record in the user table
            UserTenantModel existingModel = await this.GetAsync(input);
            if (existingModel != null)
            {
                // If this record already exists, return it without continuing with the insert operation
                throw new StorageException
                (
                    $"That UserTenant record already exists with value {existingModel.Roles}." +
                    " Use PUT instead to update this setting instead."
                );
            }
            UserTenantModel user = new UserTenantModel(input);
            // Insert the user record. Return the user model from the user table insert
            TableOperation insertOperation = TableOperation.Insert(user);
            TableResult userInsert = await this._tableHelper.ExecuteOperationAsync(this.tableName, insertOperation);
            return userInsert.Result as UserTenantModel;  // cast to UserTenantModel to parse results
        }

        /// <summary>
        /// Update a user record
        /// </summary>
        /// <param name="input">UserTenantInput with a userId, tenant, and rolelist</param>
        /// <returns></returns>
        public async Task<UserTenantModel> UpdateAsync(UserTenantInput input)
        {
            UserTenantModel model = new UserTenantModel(input);
            if (model.RoleList != null && !model.RoleList.Any())
            {
                // If the RoleList of the model is empty, throw an exception. The RoleList is the only updateable feature of the UserTenant Table
                throw new ArgumentException("The UserTenant update model must contain a serialized role array.");
            }
            model.ETag = "*";  // An ETag is required for updating - this allows any etag to be used
            TableOperation replaceOperation = TableOperation.InsertOrMerge(model);
            TableResult replace = await this._tableHelper.ExecuteOperationAsync(this.tableName, replaceOperation);
            return replace.Result as UserTenantModel;
        }

        /// <summary>
        /// Delete a User record in the UserTenantContainer using the given userId and current tenant
        /// </summary>
        /// <param name="input">UserTenantInput with a userId</param>
        /// <returns></returns>
        public async Task<UserTenantModel> DeleteAsync(UserTenantInput input)
        {
            // Get a list of all user models for this user id - we will pick the one matching the current tenant to delete
            UserTenantModel user = await this.GetAsync(input);
            TableOperation deleteOperation = TableOperation.Delete(user);

            // delete the record and return the deleted user model
            TableResult deleteUser = await this._tableHelper.ExecuteOperationAsync(this.tableName, deleteOperation);
            return deleteUser.Result as UserTenantModel;
        }

        /// <summary>
        /// Delete the tenant from all users
        /// </summary>
        /// <returns></returns>
        public async Task<UserTenantListModel> DeleteAllAsync(UserTenantInput input)
        {
            // get all rows where the tenant exists
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, input.tenant));
            TableQuerySegment resultSegment = await this._tableHelper.QueryAsync(this.tableName, query, null);
            List<UserTenantModel> tenantRows = resultSegment.Results.Select(t => (UserTenantModel)t).ToList();  // cast to a UserTenantModel list to easily parse result

            // delete the rows
            var deleteTasks = tenantRows.Select(row =>
            {
                UserTenantInput deleteInput = new UserTenantInput
                {
                    userId = row.PartitionKey,
                    tenant = input.tenant
                };
                return this.DeleteAsync(deleteInput);
            });
            var deletionResult = await Task.WhenAll(deleteTasks);
            return new UserTenantListModel("Delete", deletionResult.ToList());
        }
    }
}