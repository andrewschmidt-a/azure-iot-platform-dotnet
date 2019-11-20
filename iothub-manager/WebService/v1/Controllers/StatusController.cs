// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.IoTHubManager.WebService.Runtime;
using Mmm.Platform.IoT.IoTHubManager.WebService.v1.Models;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.WebService.v1.Filters;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IConfig config;
        private readonly IStatusService statusService;

        public StatusController(IConfig config, IStatusService statusService)
        {
            this.statusService = statusService;
            this.config = config;
        }

        [HttpGet]
        public async Task<StatusApiModel> GetAsync()
        {
            bool authRequired = this.config.ClientAuthConfig.AuthRequired;
            var serviceStatus = await this.statusService.GetStatusAsync(authRequired);
            var result = new StatusApiModel(serviceStatus);

            result.Properties.Add("AuthRequired", authRequired.ToString());
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
