using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tenant_manager.Helpers;
using tenant_manager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.ApplicationModel.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace tenant_manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private IConfiguration _config;
        private KeyVaultHelper keyVaultHelper;

        public TenantController(IConfiguration config)
        {
            this._config = config;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
        }

        // POST api/tenant
        [HttpPost]
        public string Post()
        {
            /* Creates a new tenant */

            // Load variables from app config
            string subscriptionId = this._config["Global:subscriptionId"];
            string rgName = this._config["Global:resourceGroup"];

            // Load secrets from key vault
            var secretTasks = new Task<string>[] {
                keyVaultHelper.getSecretAsync("storageAccountConnectionString"),
                keyVaultHelper.getSecretAsync("createIotHubWebHookUrl"),
                keyVaultHelper.getSecretAsync("updateFunctionsWebHookUrl")
            };
            Task.WaitAll(secretTasks);
            
            string storageAccountConnectionString = secretTasks[0].Result;
            string createIotHubWebHookUrl = secretTasks[1].Result;
            string updateFunctionsWebHookUrl = secretTasks[2].Result;

            // Generate new tenant information
            string tenantGuid = Guid.NewGuid().ToString();
            string iotHubName = "iothub-" + tenantGuid.Substring(0, 8);
            string telemetryCollectionName = "telemetry-" + tenantGuid.Substring(0, 8);
            string twinChangeCollectionName = "twin-change-" + tenantGuid.Substring(0, 8);
            string lifecycleCollectionName = "lifecycle-" + tenantGuid.Substring(0, 8);

            // Create a new tenant and save it to table storage
            var tenant = new TenantModel(tenantGuid, iotHubName, telemetryCollectionName);
            TenantTableHelper.WriteNewTenantToTableAsync(storageAccountConnectionString, "tenant", tenant);

            // Write to app config?
            // var appConfigClient = new ConfigurationClient(appConfigurationConnectionString);
            // // Write the new IoT Hub connection string to app configuration
            // appConfigClient.Set(new ConfigurationSetting(string.Format("tenant:{0}:iotHubConnectionString", tenantGuid), iotHubConnectionString));
            // //Write the new cosmos db collection names to app configuration
            // appConfigClient.Set(new ConfigurationSetting(string.Format("tenant:{0}:telemetryCosmosCollectionName", tenantGuid), cosmosTelemetryCollectionName));

            // Trigger run book to create a new IoT Hub
            HttpClient client = new HttpClient();
            var authToken = TokenHelper.GetServicePrincipleToken();

            var requestBody = new
            {   
                tenantId = tenantGuid,
                iotHubName = iotHubName,
                token = authToken
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            client.PostAsync(createIotHubWebHookUrl, bodyContent);

            // Trigger run book to update the azure functions
            var requestBody2 = new
            {   
                tenantId = tenantGuid,
                telemetryCollectionName = telemetryCollectionName,
                twinChangeCollectionName = twinChangeCollectionName,
                lifecycleCollectionName = lifecycleCollectionName,
                token = authToken
            };

            bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody2), Encoding.UTF8, "application/json");
            client.PostAsync(updateFunctionsWebHookUrl, bodyContent);

            return "Your tenant is being created. Your tenant GUID is: " + tenantGuid;
        }

        // GET api/tenantready/<tenantId>
        [HttpGet("{tenantId}")]
        public TenantModel Get(string tenantId)
        {
            /* Returns information for a tenant */

            // Load variables from key vault
            var storageAccountConnectionString = this.keyVaultHelper.getSecretAsync("storageAccountConnectionString").Result;

            // Load the tenant from table storage
            TenantModel tenant = TenantTableHelper.ReadTenantFromTableAsync(storageAccountConnectionString, "tenant", tenantId).Result;            

            return tenant;
        }
    }
}
