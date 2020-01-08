// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.AsaManager.WebService.Models;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.AsaManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : ControllerBase
    {
        private readonly AppConfig config;
        private readonly IStatusService statusService;

        public StatusController(AppConfig config, IStatusService statusService)
        {
            this.config = config;
            this.statusService = statusService;
        }
        [HttpGet]
        public async Task<StatusApiModel> GetAsync()
        {
            try
            {
                bool authRequired = config.Global.ClientAuth.AuthRequired;
                var serviceStatus = await this.statusService.GetStatusAsync(authRequired);
                var result = new StatusApiModel(await this.statusService.GetStatusAsync(false));

                result.Properties.Add("AuthRequired", authRequired.ToString());
                result.Properties.Add("Port", config.DeviceTelemetryService.Port.ToString());
                return result;
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
