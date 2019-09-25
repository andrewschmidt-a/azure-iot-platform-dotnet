using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using ILogger = Microsoft.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
{
    public class TableStorageHelper
    {
        private readonly CloudStorageAccount storageAccount;
        private CloudTableClient client;

        public TableStorageHelper(string storageAccountConnectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            this.client = this.storageAccount.CreateCloudTableClient();
        }

        private async Task<CloudTable> GetTableAsync(string tableName)
        {
            try
            {
                CloudTable table = this.client.GetTableReference(tableName);
                try
                {
                    await table.CreateIfNotExistsAsync();
                }
                catch (StorageException e)
                {
                    throw new Exception($"An error occurred during table.CreateIfNotExistsAsync for the {tableName} table.", e);
                }
                return table;
            }
            catch (StorageException se)
            {
                throw new Exception($"An error occurred while attempting to get the {tableName} table and checking if it needed to be created.", se);
            }
        }

        private async Task<TableResult> ExecuteTableOperationAsync(CloudTable table, TableOperation operation)
        {
            try
            {
                return await table.ExecuteAsync(operation);
            }
            catch (StorageException se)
            {
                throw new Exception($"Unable to perform the requested table operation {operation.OperationType}", se);
            }
        }

        public async Task<TableResult> WriteToTableAsync<T>(string tableName, T entity) where T: TableEntity
        {
            CloudTable table = await this.GetTableAsync(tableName);
            TableOperation insertOperation = TableOperation.Insert(entity);
            return await this.ExecuteTableOperationAsync(table, insertOperation);
        }

        public async Task<T> ReadFromTableAsync<T>(string tableName, string partitionKey, string rowKey) where T: TableEntity
        {
            CloudTable table = await this.GetTableAsync(tableName);
            TableOperation readOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult result = await this.ExecuteTableOperationAsync(table, readOperation);
            try
            {
                return (T)result.Result;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to transform the table result {result.ToString()} to the requested entity type", e);
            }
        }

        public async Task<TableResult> DeleteEntityAsync<T>(string tableName, T entity) where T: TableEntity
        {
            CloudTable table = await this.GetTableAsync(tableName);
            TableOperation deleteOperation = TableOperation.Delete(entity);
            return await this.ExecuteTableOperationAsync(table, deleteOperation);
        }
    }
}