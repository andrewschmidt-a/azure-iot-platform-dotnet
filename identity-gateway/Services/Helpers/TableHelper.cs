using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Helpers
{
    public class TableHelper : ITableHelper
    {
        private readonly string storageAccountConnectionString;

        public TableHelper(IServicesConfig config)
        {
            this.storageAccountConnectionString = config.StorageAccountConnectionString;
        }

        public async Task<CloudTable> GetTableAsync(string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
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

        public async Task<TableResult> ExecuteOperationAsync(string tableName, TableOperation operation)
        {
            CloudTable table = await this.GetTableAsync(tableName);
            return await table.ExecuteAsync(operation);
        }
    }
}