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
        // POST api/tenant
        [HttpPost]
        public string Post()
        {
            /* Creates a new tenant */

            // Load variables from key vault
            string subscriptionId = "c36fb2f8-f98d-40d0-90a9-d65e93acb428";
            string rgName = "rg-crslbbiot-odin-dev";
            string appConfigurationConnectionString = "Endpoint=https://configinfo.azconfig.io;Id=0-l3-s0:yS689kB3EvQhGLxmJ7Aa;Secret=BTx9mm1sZht6JI71g2gYgZ/Vxop14LUDZ831fqtmhSY=";
            string storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=functiondefinition;AccountKey=bTwW6MmAElpi6U1s2lvC9C4QCW1jC6AVURjmmvrDBT9pmKocJCN7DVt21GW8G4SL0NM+HAyXu2pwTGiAJmNMcA==;EndpointSuffix=core.windows.net";
            string createIotHubWebHookUrl = "https://s25events.azure-automation.net/webhooks?token=hWvfeFes46W0F0WREyD55NdKF58Oih0jlGpTHhZWewY%3d";
            string updateFunctionsWebHookUrl = "https://s25events.azure-automation.net/webhooks?token=VXIbEy02Wm2DNlJsI42%2fTjGYlnXbhdT8zfV1%2baHAsvk%3d";

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
            string storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=functiondefinition;AccountKey=bTwW6MmAElpi6U1s2lvC9C4QCW1jC6AVURjmmvrDBT9pmKocJCN7DVt21GW8G4SL0NM+HAyXu2pwTGiAJmNMcA==;EndpointSuffix=core.windows.net";

            // Load the tenant from table storage
            TenantModel tenant = TenantTableHelper.ReadTenantFromTableAsync(storageAccountConnectionString, "tenant", tenantId).Result;            

            return tenant;
        }
    }
}
