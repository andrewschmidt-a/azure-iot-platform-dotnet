using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantReadyController : ControllerBase
    {
        private IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private KeyVaultHelper keyVaultHelper;

        public TenantReadyController(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
        }
        
        // GET api/tenantready/<tenantId>
        [HttpGet("{tenantId}", Name = "Get")]
        public IActionResult Get(string tenantId)
        {
            /* Checks whether a tenant currently exists or not */

            // Load variables from key vault
            var storageAccountConnectionString = this.keyVaultHelper.getSecretAsync("storageAccountConnectionString").Result;

            // Verify that the user has access to the specified tenant
            var userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
            var userTenant = TableStorageHelper<UserTenantModel>.ReadFromTableAsync(storageAccountConnectionString, "user", userId, tenantId).Result;

            if (userTenant == null) {
                // User does not have access
                return Unauthorized();
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = TableStorageHelper<TenantModel>.ReadFromTableAsync(storageAccountConnectionString, "tenant", partitionKey, tenantId).Result;

            // Check whether the tenant is done deploying or not
            if (tenant != null && tenant.IsIotHubDeployed) {
                return Ok(true);
            }

            return Ok(false);
        }
    }
}
