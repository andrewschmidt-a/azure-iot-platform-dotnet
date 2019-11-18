using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using Mmm.Platform.IoT.Common.Services.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Helpers
{
    public class TableStorageHelper : IStatusOperation
    {
        private const string STATUS_TABLE_NAME = "tenant";

        private readonly CloudStorageAccount storageAccount;
        private CloudTableClient client;

        public TableStorageHelper(IServicesConfig config)
        {
            this.storageAccount = CloudStorageAccount.Parse(config.StorageAccountConnectionString);
            this.client = this.storageAccount.CreateCloudTableClient();
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                CloudTable table = await this.GetTableAsync(STATUS_TABLE_NAME);
                if (table != null)
                {
                    return new StatusResultServiceModel(true, "Alive and well!");
                }
                else
                {
                    return new StatusResultServiceModel(false, $"Could not get the {STATUS_TABLE_NAME} table from table storage. Storage check failed.");
                }
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, $"Table Storage check failed: {e.Message}");
            }
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

        public async Task<TableResult> WriteToTableAsync<T>(string tableName, T entity) where T : TableEntity
        {
            CloudTable table = await this.GetTableAsync(tableName);
            TableOperation insertOperation = TableOperation.Insert(entity);
            return await this.ExecuteTableOperationAsync(table, insertOperation);
        }

        public async Task<T> ReadFromTableAsync<T>(string tableName, string partitionKey, string rowKey) where T : TableEntity
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

        public async Task<TableResult> DeleteEntityAsync<T>(string tableName, T entity) where T : TableEntity
        {
            CloudTable table = await this.GetTableAsync(tableName);
            TableOperation deleteOperation = TableOperation.Delete(entity);
            return await this.ExecuteTableOperationAsync(table, deleteOperation);
        }
    }
}