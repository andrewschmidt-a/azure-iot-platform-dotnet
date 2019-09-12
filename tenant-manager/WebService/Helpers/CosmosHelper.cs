using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
{
    public class CosmosHelper
    {
        private IConfiguration _config;

        public CosmosHelper(IConfiguration _config)
        {
            this._config = _config;
        }
        public string CreateCosmosDbCollection(string token, string tenantGuid, string collectionPrefix)
        {
            /* Creates a new Cosmos DB colletion. An empty string is returned on failure. */

            string databaseAccount = this._config["TenantManagerService:cosmosAccount"];
            string dbId = this._config["TenantManagerService:databaseName"];
            string collectionName = collectionPrefix + "-" + token;

            // Create an Http Client
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("x-ms-version", "2015-12-16");

            // Cosmos collection description
            var description = new
            {   
                id = collectionName
            };
            var json = JsonConvert.SerializeObject(description, Formatting.Indented);

            // Submit the REST API request to create the Cosmos db collection
            var content = new StringContent(JsonConvert.SerializeObject(description), Encoding.UTF8, "application/json");
            var requestUri = string.Format("https://{0}.documents.azure.com/dbs/{1}/colls", databaseAccount, dbId);
            var result = client.PostAsync(requestUri, content).Result;

            // Check the result status code
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed {0}", result.Content.ReadAsStringAsync().Result);
                // Return an empty string to indicate failure
                return "";
            }

            return collectionName;
        }
    }
}