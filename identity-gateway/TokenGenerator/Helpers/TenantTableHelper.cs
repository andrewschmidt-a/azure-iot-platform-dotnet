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
        public static async Task<TenantModel> GetUserTenantInfo(string storageAccountConnectionString, string tenantGuid, string userId)
        {
            /* Writes a new tenant object to a storage table */

            // Get a reference to the storage table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference("user");

            // Create the table if it doesn't already exist
            await table.CreateIfNotExistsAsync();
            //var test = new TenantModel(userId, tenantGuid);
            //test.Roles = "[]";
            // Create and save the new tenant
            TableOperation fetchOp = TableOperation.Retrieve<TenantModel>(userId, tenantGuid);
            //TableOperation insertOp = TableOperation.Insert(test);

            //await table.ExecuteAsync(insertOp);
            var result = await table.ExecuteAsync(fetchOp);
            var tenant = result.Result as TenantModel;

            return tenant;
        }
    }
}