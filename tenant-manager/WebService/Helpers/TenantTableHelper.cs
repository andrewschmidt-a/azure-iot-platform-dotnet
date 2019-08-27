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
using tenant_manager.Models;
using Microsoft.Azure.Documents;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
{
    public class TenantTableHelper
    {
        public static async Task WriteNewTenantToTableAsync(string storageAccountConnectionString, string tableName, TenantModel tenant)
        {
            /* Writes a new tenant object to a storage table */

                // Get a reference to the storage table
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
                CloudTableClient client = storageAccount.CreateCloudTableClient();
                CloudTable table = client.GetTableReference(tableName);
            try
            {
                // Create the table if it doesn't already exist
                await table.CreateIfNotExistsAsync();

                // Create and save the new tenant
                TableOperation insertOp = TableOperation.Insert(tenant);
                TableResult tableResult = await table.ExecuteAsync(insertOp);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task<TenantModel> ReadTenantFromTableAsync(string storageAccountConnectionString, string tableName, string tenantId)
        {
            /* Return a tenant from table storage */
            try { 
            // Get a reference to the storage table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);

            // Get the tenant object
            string partitionKey = tenantId.Substring(0, 1);
            TableOperation readOp = TableOperation.Retrieve<TenantModel>(partitionKey, tenantId);
            TableResult tableResult = await table.ExecuteAsync(readOp);
            TenantModel result = tableResult.Result as TenantModel;
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