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
    public class TenantReadyController : ControllerBase
    {
        // GET api/tenantready/<tenantId>
        [HttpGet("{tenantId}", Name = "Get")]
        public bool Get(string tenantId)
        {
            /* Checks whether a tenant currently exists or not */

            // Load variables from key vault
            string storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=functiondefinition;AccountKey=bTwW6MmAElpi6U1s2lvC9C4QCW1jC6AVURjmmvrDBT9pmKocJCN7DVt21GW8G4SL0NM+HAyXu2pwTGiAJmNMcA==;EndpointSuffix=core.windows.net";

            // Load the tenant from table storage
            TenantModel tenant = TenantTableHelper.ReadTenantFromTableAsync(storageAccountConnectionString, "tenant", tenantId).Result;

            // Check whether the tenant is done deploying or not
            if (tenant != null && tenant.IsIotHubDeployed && tenant.AreFunctionsUpdated) {
                return true;
            }

            return false;
        }
    }
}
