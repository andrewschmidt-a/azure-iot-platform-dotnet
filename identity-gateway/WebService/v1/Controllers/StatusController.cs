using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.IdentityGateway.Services.Runtime;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.WebService.v1.Filters;
using Mmm.Platform.IoT.IdentityGateway.WebService.Models;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.v1.Controllers
{
    [Route("v1/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IServicesConfig config;
        private readonly IStatusService statusService;

        public StatusController(IServicesConfig config, IStatusService statusService)
        {
            this.config = config;
            this.statusService = statusService;
        }
        [HttpGet]
        public async Task<StatusApiModel> GetAsync()
        {
            var result = new StatusApiModel(await this.statusService.GetStatusAsync(false));

            result.Properties.Add("Port", this.config.Port.ToString());
            return result;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return new StatusCodeResult(200);
        }
    }
}