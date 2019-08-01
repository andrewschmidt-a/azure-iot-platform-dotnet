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
using tenant_manager.Models;

namespace tenant_manager.Helpers
{
    public class TenantTableHelper
    {
        public static async void WriteNewTenantToTableAsync(string storageAccountConnectionString, string tableName, TenantModel tenant)
        {
            /* Writes a new tenant object to a storage table */

            // Get a reference to the storage table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);

            // Create the table if it doesn't already exist
            await table.CreateIfNotExistsAsync();

            // Create and save the new tenant
            TableOperation insertOp = TableOperation.Insert(tenant);
            await table.ExecuteAsync(insertOp);
        }

        public static async Task<TenantModel> ReadTenantFromTableAsync(string storageAccountConnectionString, string tableName, string tenantId)
        {
            /* Return a tenant from table storage */

            // Get a reference to the storage table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);

            // Get the tenant object
            string partitionKey = tenantId.Substring(0, 1);
            TableOperation readOp = TableOperation.Retrieve<TenantModel>(partitionKey, tenantId);
            TableResult tableResult = await table.ExecuteAsync(readOp);
            TenantModel result = (TenantModel) tableResult.Result;

            return result;
        }
    }
}