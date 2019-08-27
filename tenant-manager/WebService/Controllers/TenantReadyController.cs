using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.ApplicationModel.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantReadyController : ControllerBase
    {
        private IConfiguration _config;
        private KeyVaultHelper keyVaultHelper;

        public TenantReadyController(IConfiguration config)
        {
            this._config = config;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
        }
        
        // GET api/tenantready/<tenantId>
        [HttpGet("{tenantId}", Name = "Get")]
        public bool Get(string tenantId)
        {
            /* Checks whether a tenant currently exists or not */

            // Load variables from key vault
            var storageAccountConnectionString = this.keyVaultHelper.getSecretAsync("storageAccountConnectionString").Result;

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
