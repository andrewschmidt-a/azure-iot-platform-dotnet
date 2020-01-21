using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Controllers
{
    [Route("v1/[controller]")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : ControllerBase
    {
        private readonly IStatusService statusService;

        public StatusController(IStatusService statusService)
        {
            this.statusService = statusService;
        }

        [HttpGet]
        public async Task<StatusApiModel> GetAsync()
        {
            try
            {
                return new StatusApiModel(await this.statusService.GetStatusAsync(), "IoTHub Manager");
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while attempting to get the service status", e);
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return new StatusCodeResult(200);
        }
    }
}
