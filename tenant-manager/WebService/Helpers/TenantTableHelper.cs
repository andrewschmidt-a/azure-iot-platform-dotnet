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
        public static async void WriteNewTenantToTableAsync(string storageAccountConnectionString, string tenantGuid, string tableName, string iotHubConnectionString)
        {
            /* Writes a new tenant object to a storage table */

            // Get a reference to the storage table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);

            // Create the table if it doesn't already exist
            await table.CreateIfNotExistsAsync();

            // Create and save the new tenant
            var tenant = new TenantModel(tenantGuid, iotHubConnectionString);
            TableOperation insertOp = TableOperation.Insert(tenant);
            await table.ExecuteAsync(insertOp);
        }
    }
}