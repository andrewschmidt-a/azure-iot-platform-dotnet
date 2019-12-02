using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.TenantManager.Services;

namespace Mmm.Platform.IoT.TenantManager.WebService.Controllers
{
    [Route("api/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class TenantReadyController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantContainer _tenantContainer;
        private readonly ILogger _log;

        public TenantReadyController(IHttpContextAccessor httpContextAccessor, ITenantContainer tenantContainer, ILogger log)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._tenantContainer = tenantContainer;
            this._log = log;
        }

        // GET api/tenantready/<tenantId>
        [HttpGet("{tenantId}")]
        public async Task<bool> GetAsync(string tenantId)
        {
            return await this._tenantContainer.TenantIsReadyAsync(tenantId);
        }
    }
}
