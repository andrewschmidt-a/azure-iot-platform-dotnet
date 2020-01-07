// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.Config.WebService.v1.Models;

namespace Mmm.Platform.IoT.Config.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
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
            var result = new StatusApiModel(await this.statusService.GetStatusAsync(false));

            result.Properties.Add("AuthRequired", config.Global.ClientAuth?.AuthRequired.ToString());
            result.Properties.Add("Port", config.ConfigService.WebServicePort.ToString());
            return result;
        }
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return new StatusCodeResult(200);
        }
    }
}

