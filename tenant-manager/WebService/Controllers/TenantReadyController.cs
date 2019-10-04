using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Filters;
using MMM.Azure.IoTSolutions.TenantManager.Services.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;
using ILogger = MMM.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class TenantReadyController : ControllerBase
    {
        private const string STORAGE_ACCOUNT_CONN_STRING_KEY = "storageAccountConnectionString";
        private const string TENANT_TABLE_ID = "tenant";
        private const string USER_TABLE_ID = "user";

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
        public async Task<IActionResult> GetAsync(string tenantId)
        {
            /* Checks whether a tenant currently exists or not */

            // Load variables from key vault
            var storageAccountConnectionString = await this.keyVaultHelper.GetSecretAsync(STORAGE_ACCOUNT_CONN_STRING_KEY);
            // Create a table storage helper now that we have the storage account conn string
            var tableStorageHelper = new TableStorageHelper(storageAccountConnectionString); 

            var userId = "";
            try
            {
                userId = this._httpContextAccessor.HttpContext.Request.GetCurrentUserObjectId();
                if (String.IsNullOrEmpty(userId))
                {
                    throw new NullReferenceException("The UserId retrieved from Http Context was null or empty.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve the userId from the httpContextAccessor", e);
            }

            // Verify that the user has access to the specified tenantx
            var userTenant = await tableStorageHelper.ReadFromTableAsync<UserTenantModel>(USER_TABLE_ID, userId, tenantId);
            if (userTenant == null) {
                // User does not have access
                return Unauthorized();
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, partitionKey, tenantId);

            // Check whether the tenant is done deploying or not
            if (tenant != null && tenant.IsIotHubDeployed) {
                return Ok(true);
            }

            return Ok(false);
        }
    }
}
