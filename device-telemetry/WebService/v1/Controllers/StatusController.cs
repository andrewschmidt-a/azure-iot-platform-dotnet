// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Models;

namespace Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly AppConfig config;
        private readonly IStatusService statusService;

        public StatusController(AppConfig config, IStatusService statusService)
        {
            this.statusService = statusService;
            this.config = config;
        }

        [HttpGet]
        public async Task<StatusApiModel> GetAsync()
        {
            bool authRequired = this.config.Global.ClientAuth.AuthRequired;
            var serviceStatus = await this.statusService.GetStatusAsync(authRequired);
            var result = new StatusApiModel(serviceStatus);

            result.Properties.Add("AuthRequired", authRequired.ToString());
            result.Properties.Add("Port", this.config.TelemetryService.WebServicePort.ToString());
            return result;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return new StatusCodeResult(200);
        }
    }
}
