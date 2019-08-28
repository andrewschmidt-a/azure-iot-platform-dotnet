using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
{
    public class TableStorageHelper<T> where T: TableEntity
    {
        public static async void WriteToTableAsync(string storageAccountConnectionString, string tableName, T tableEntity)
        {
            /* Writes a new table entity object to a storage table */

            // Get a reference to the storage table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);

            // Create the table if it doesn't already exist
            await table.CreateIfNotExistsAsync();

            // Create and save the new table entity
            TableOperation insertOp = TableOperation.Insert(tableEntity);
            await table.ExecuteAsync(insertOp);
        }

        public static async Task<T> ReadFromTableAsync(string storageAccountConnectionString, string tableName, string partitionKey, string rowKey)
        {
            /* Return an object from table storage */

            // Get a reference to the storage table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);

            // Get the object
            TableOperation readOp = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult tableResult = await table.ExecuteAsync(readOp);
            T result = (T) tableResult.Result;

            return result;
        }
    }
}