using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tenant_manager.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.ApplicationModel.Configuration;
using Newtonsoft.Json.Linq;

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

            // Generate a GUID to identify the new tenant
            string tenantGuid = Guid.NewGuid().ToString();

            // Load the subscription and resource group
            string subscriptionId = "c36fb2f8-f98d-40d0-90a9-d65e93acb428";
            string rgName = "rg-crslbbiot-odin-dev";

            var appConfigClient = new ConfigurationClient("Endpoint=https://configinfo.azconfig.io;Id=0-l3-s0:yS689kB3EvQhGLxmJ7Aa;Secret=BTx9mm1sZht6JI71g2gYgZ/Vxop14LUDZ831fqtmhSY=");

            // Get the service principle token for authorizing the REST API calls
            string token = TokenHelper.GetServicePrincipleToken();
            
            if (token == "")
            {
                return "Authenticating with the service principle failed.";
            }

            // Provision a new IoT Hub
            string iotHubConnectionString = IotHubHelper.CreateIotHub(token, tenantGuid, subscriptionId, rgName);

            // Write the new IoT Hub connection string to app configuration
            appConfigClient.Set(new ConfigurationSetting(string.Format("tenant:{0}:iotHubConnectionString", tenantGuid), iotHubConnectionString));

            // Create a new cosmos db collections
            string cosmosTelemetryCollectionName = CosmosHelper.CreateCosmosDbCollection(token, tenantGuid, "telemetry");

            // Write the new cosmos db collection names to app configuration
            appConfigClient.Set(new ConfigurationSetting(string.Format("tenant:{0}:telemetryCosmosCollectionName", tenantGuid), cosmosTelemetryCollectionName));

            // Update 3 azure functions to write to new cosmos db collections

            return "Success";
        }
    }
}
