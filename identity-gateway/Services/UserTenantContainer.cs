using System;
using System.Linq;
using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public class UserTenantContainer : UserContainer, IUserContainer<UserTenantModel, UserTenantInput> 
    {
        public UserTenantContainer()
        {
        }
        
        public UserTenantContainer(ITableHelper tableHelper) : base(tableHelper)
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
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, input.UserId));
            TableQuerySegment resultSegment = await this._tableHelper.QueryAsync(this.TableName, query, null);
            return new UserTenantListModel("GetTenants", resultSegment.Results.Select(t => (UserTenantModel)t).ToList());
        }


        /// <summary>
        /// get all users for a tenant
        /// </summary>
        /// <param name="input">UserTenantInput with the tenant param</param>
        /// <returns></returns>
        public virtual async Task<UserTenantListModel> GetAllUsersAsync(UserTenantInput input)
        {
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, input.Tenant));
            TableQuerySegment resultSegment = await this._tableHelper.QueryAsync(this.TableName, query, null);
            return new UserTenantListModel("GetUsers", resultSegment.Results.Select(t => (UserTenantModel)t).ToList());
        }
        /// <summary>
        /// Get a single tenant for the user
        /// </summary>
        /// <param name="input">UserTenantInput with a userid</param>
        /// <returns></returns>
        public virtual async Task<UserTenantModel> GetAsync(UserTenantInput input)
        {
            TableOperation retrieveUserTenant = TableOperation.Retrieve<UserTenantModel>(input.UserId, input.Tenant);
            TableResult result = await this._tableHelper.ExecuteOperationAsync(this.TableName, retrieveUserTenant);
            return result.Result as UserTenantModel;
        }

        /// <summary>
        /// Create a User record in the UserTenantContainer using the given userId and current tenant
        /// </summary>
        /// <param name="input">UserTenantInput with a userId</param>
        /// <returns></returns>
        public virtual async Task<UserTenantModel> CreateAsync(UserTenantInput input)
        {
            // If UserId is null then make it up
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
                    " Use PUT instead to update this setting instead."
                );
            }
            UserTenantModel user = new UserTenantModel(input);
            // Insert the user record. Return the user model from the user table insert
            TableOperation insertOperation = TableOperation.Insert(user);
            TableResult userInsert = await this._tableHelper.ExecuteOperationAsync(this.TableName, insertOperation);
            return userInsert.Result as UserTenantModel;  // cast to UserTenantModel to parse results
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
            TableOperation replaceOperation = TableOperation.InsertOrMerge(model);
            TableResult replace = await this._tableHelper.ExecuteOperationAsync(this.TableName, replaceOperation);
            return replace.Result as UserTenantModel;
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
            TableOperation deleteOperation = TableOperation.Delete(user);

            // delete the record and return the deleted user model
            TableResult deleteUser = await this._tableHelper.ExecuteOperationAsync(this.TableName, deleteOperation);
            return deleteUser.Result as UserTenantModel;
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