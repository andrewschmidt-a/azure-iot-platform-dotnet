// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.TenantManager.WebService.Models;

namespace Mmm.Platform.IoT.TenantManager.WebService.Controllers
{
    [Route("v1/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
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
        public async Task<StatusModel> GetAsync()
        {
            try
            {
                var result = new StatusModel(await this.statusService.GetStatusAsync(false));
                result.Properties.Add("Port", this.config.TenantManagerService.Port.ToString());
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
