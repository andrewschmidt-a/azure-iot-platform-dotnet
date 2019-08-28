using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
{
    public class TableStorageHelper<T> where T: TableEntity
    {
        public static async Task WriteToTableAsync(string storageAccountConnectionString, string tableName, T tableEntity)
        {
            /* Writes a new table entity object to a storage table */

            // Get a reference to the storage table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);
            
            try
            {
                // Create the table if it doesn't already exist
                await table.CreateIfNotExistsAsync();

                // Create and save the new table entity
                TableOperation insertOp = TableOperation.Insert(tableEntity);
                TableResult tableResult = await table.ExecuteAsync(insertOp);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task<T> ReadFromTableAsync(string storageAccountConnectionString, string tableName, string partitionKey, string rowKey)
        {
            /* Return a object from table storage */

            try { 
                // Get a reference to the storage table
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
                CloudTableClient client = storageAccount.CreateCloudTableClient();
                CloudTable table = client.GetTableReference(tableName);

                // Get the object
                TableOperation readOp = TableOperation.Retrieve<T>(partitionKey, rowKey);
                TableResult tableResult = await table.ExecuteAsync(readOp);
                T result = (T) tableResult.Result;
                if (result != null)
                {
                    Console.WriteLine("\t{0}\t{1}, result.PartitionKey, result.RowKey");
                }
                return result;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task DeleteEntityAsync(string storageAccountConnectionString, string tableName, TenantModel deleteEntity)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);
            try
            {
                if (deleteEntity == null)
                {
                    throw new ArgumentNullException("deleteEntity");
                }

                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                TableResult result = await table.ExecuteAsync(deleteOperation);

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Delete Operation: " + result.RequestCharge);
                }
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}