using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.TenantManager.Services;
using Mmm.Platform.IoT.TenantManager.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.TenantManager.WebService.Controllers
{
    [Route("api/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class TenantController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantContainer _tenantContainer;
        private readonly ILogger _log;

        public TenantController(IHttpContextAccessor httpContextAccessor, ITenantContainer tenantContainer, ILogger log)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._tenantContainer = tenantContainer;
            this._log = log;
        }

        // POST api/tenant
        [HttpPost]
        public async Task<string> PostAsync()
        {
            /* Creates a new tenant */
            // Generate new tenant Id
            string tenantGuid = Guid.NewGuid().ToString();
            try
            {
                var response = await this._tenantContainer.CreateTenantAsync(tenantGuid);
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception e)
            {
                // If there is an error while creating the new tenant - delete all of the created tenant resources
                // this may not be able to delete iot hub - due to the long running process
                var deleteResponse = await this._tenantContainer.DeleteTenantAsync(tenantGuid, false);
                this._log.Info("The Tenant was unable to be created properly. To ensure the failed tenant does not consume resources, some of its resources were deleted after creation failed.", () => new { deleteResponse });
                throw e;
            }
        }

        // GET api/tenant/<tenantId>
        [HttpGet("{tenantId}")]
        [Authorize("ReadAll")]
        public async Task<TenantModel> GetAsync(string tenantId)
        {
            return await this._tenantContainer.GetTenantAsync(tenantId);
        }

        [HttpDelete("{tenantId}")]
        [Authorize("DeleteTenant")]
        public async Task<string> DeleteAsync(string tenantId, [FromQuery] bool ensureFullyDeployed = true)
        {
            var response = await this._tenantContainer.DeleteTenantAsync(tenantId, ensureFullyDeployed);
            return JsonConvert.SerializeObject(response);
        }
    }
}
