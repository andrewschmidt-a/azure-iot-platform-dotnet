using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace IdentityGateway.Services.Helpers
{
    public class TableHelper
    {
        private readonly KeyVaultHelper _keyVaultHelper;
        private readonly string storageAccountConnectionStringKey;

        public TableHelper(IConfiguration config)
        {
            this._keyVaultHelper = new KeyVaultHelper(config);
            this.storageAccountConnectionStringKey = config["StorageAccountConnectionStringKeyVaultSecret"];
        }

        private CloudStorageAccount storageAccount
        {
            get
            {
                string tenantStorageAccountConnectionString = this._keyVaultHelper.getSecretAsync(this.storageAccountConnectionStringKey).GetAwaiter().GetResult();
                return CloudStorageAccount.Parse(tenantStorageAccountConnectionString); 
            }
        }

        public async Task<CloudTable> GetTableAsync(string tableName)
        {
            CloudTableClient client = this.storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);

            // Create the table if it doesn't already exist
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<TableQuerySegment> QueryAsync(string tableName, TableQuery query, TableContinuationToken token)
        {
            CloudTable table = await this.GetTableAsync(tableName);
            return await table.ExecuteQuerySegmentedAsync(query, token);
        }

        public async Task<TableResult> ExecuteOperationAsync(string tableName, TableOperation retrievalOperation)
        {
            CloudTable table = await this.GetTableAsync(tableName);
            return await table.ExecuteAsync(retrievalOperation);
        }
    }
}