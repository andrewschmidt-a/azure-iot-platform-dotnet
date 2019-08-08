using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using IdentityGateway.Services.Models;
using IdentityGateway.Services.Helpers;
using IdentityGateway.AuthUtils;

namespace IdentityGateway.Services
{
    public class UserTenantTable
    {
        private readonly string storageAccountConnectionStringKey = "tenantStorageAccountConnectionString";
        private readonly string userTableName = "user";
        private readonly string userSettingsTableName = "userSettings";

        private IHttpContextAccessor _httpContextAccessor;
        private KeyVaultHelper _keyVaultHelper;

        public UserTenantTable(IHttpContextAccessor httpContextAccessor, KeyVaultHelper keyVaultHelper)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._keyVaultHelper = keyVaultHelper;
        }

        private CloudStorageAccount storageAccount
        {
            // retrieve the storage account connection string from key vault and create a storage account from the connection string
            get
            {
                string tenantStorageAccountConnectionString = this._keyVaultHelper.getSecretAsync(this.storageAccountConnectionStringKey).GetAwaiter().GetResult();
                return CloudStorageAccount.Parse(tenantStorageAccountConnectionString); 
            }
        }

        public string tenant
        {
            // get the tenant guid from the http context - this utilizes AuthUtil's request extension
            get
            {
                return this._httpContextAccessor.HttpContext.Request.GetTenant();
            }
        }

        private async Task<CloudTable> GetTable(string tableName)
        {
            CloudTableClient client = this.storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);

            // Create the table if it doesn't already exist
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<StatusResultServiceModel> PingAsync()
        {
            await this.GetTable(this.userTableName);
            return new StatusResultServiceModel(true, "Alive and Well!");
        }

        public async Task<UserModel> GetUserTenantInfoAsync(string tenantGuid, string userId)
        {
            TableOperation fetchOp = TableOperation.Retrieve<UserModel>(userId, tenantGuid);
            CloudTable userTable = await this.GetTable(this.userTableName);
            var result = await userTable.ExecuteAsync(fetchOp);
            return (UserModel)result.Result;
        }

        public async Task<List<UserModel>> GetUserTenantsAsync(string userId)
        {
            TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            CloudTable userTable = await this.GetTable(this.userTableName);
            // TODO: continuation token may need to be implemented in the future.
            // ref: https://docs.microsoft.com/en-us/azure/visual-studio/vs-storage-aspnet5-getting-started-tables
            TableQuerySegment resultSegment = await userTable.ExecuteQuerySegmentedAsync(query, null);
            return (List<UserModel>)resultSegment.Results;  // cast to a UserModel list to easily parse result
        }

        public async Task<UserModel> CreateUserTenantAsync(string userId, string roles)
        {
            // Create the user and options for creating the user record in the user table
            UserModel user = new UserModel(userId, this.tenant, roles);
            CloudTable userTable = await this.GetTable(this.userTableName);
            TableOperation insertOperation = TableOperation.Insert(user);

            // Insert the user record and the userSettings record. Return the user model from the user table insert
            TableResult userInsert = await userTable.ExecuteAsync(insertOperation);
            return (UserModel)userInsert.Result;  // cast to UserModel to parse results
        }

        public async Task<UserModel> DeleteUserTenantAsync(string userId)
        {
            CloudTable userTable = await this.GetTable(this.userTableName);

            // Get a list of all user models for this user id - we will pick the one matching the current tenant to delete
            List<UserModel> allUserTenants = await this.GetUserTenantsAsync(userId);
            UserModel user = allUserTenants.Find(i => i.RowKey == this.tenant);
            TableOperation deleteOperation = TableOperation.Delete(user);

            // delete the record and return the deleted user model
            TableResult deleteUser = await userTable.ExecuteAsync(deleteOperation);
            return (UserModel)deleteUser.Result;
        }
    }
}