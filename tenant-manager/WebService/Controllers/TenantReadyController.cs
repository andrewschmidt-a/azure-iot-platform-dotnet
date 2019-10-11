using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Filters;
using MMM.Azure.IoTSolutions.TenantManager.Services.Helpers;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    [Authorize("ReadAll")]
    public class TenantReadyController : ControllerBase
    {
        private const string TENANT_TABLE_ID = "tenant";
        private const string USER_TABLE_ID = "user";

        private readonly IServicesConfig _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TableStorageHelper _tableStorageHelper;

        public TenantReadyController(
            IServicesConfig config,
            IHttpContextAccessor httpContextAccessor,
            TableStorageHelper tableStorageHelper)
        {
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
            this._tableStorageHelper = tableStorageHelper;
        }
        
        // GET api/tenantready/<tenantId>
        [HttpGet("{tenantId}", Name = "Get")]
        public async Task<IActionResult> GetAsync(string tenantId)
        {
            /* Checks whether a tenant currently exists or not */

            // Create a table storage helper now that we have the storage account conn string

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
            var userTenant = await this._tableStorageHelper.ReadFromTableAsync<UserTenantModel>(USER_TABLE_ID, userId, tenantId);
            if (userTenant == null) {
                // User does not have access
                return Unauthorized();
            }

            // Load the tenant from table storage
            string partitionKey = tenantId.Substring(0, 1);
            TenantModel tenant = await this._tableStorageHelper.ReadFromTableAsync<TenantModel>(TENANT_TABLE_ID, partitionKey, tenantId);

            // Check whether the tenant is done deploying or not
            if (tenant != null && tenant.IsIotHubDeployed) {
                return Ok(true);
            }

            return Ok(false);
        }
    }
}
