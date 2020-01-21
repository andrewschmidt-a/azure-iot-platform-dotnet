using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.TenantManager.Services;

namespace Mmm.Platform.IoT.TenantManager.WebService.Controllers
{
    [Route("api/[controller]")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class TenantReadyController : Controller
    {
        private readonly ITenantContainer tenantContainer;
        private readonly ILogger logger;

        public TenantReadyController(ITenantContainer tenantContainer, ILogger<TenantReadyController> logger)
        {
            this.tenantContainer = tenantContainer;
            this.logger = logger;
        }

        [HttpGet("{tenantId}")]
        public async Task<bool> GetAsync(string tenantId)
        {
            return await this.tenantContainer.TenantIsReadyAsync(tenantId);
        }
    }
}