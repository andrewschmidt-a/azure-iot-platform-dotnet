// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IConfig config;
        private readonly IStatusService statusService;

        public StatusController(IConfig config, IStatusService statusService)
        {
            this.config = config;
            this.statusService = statusService;
        }

        [HttpGet]
        public async Task<StatusApiModel> GetAsync()
        {
            var result = new StatusApiModel(await this.statusService.GetStatusAsync());
            
            result.Properties.Add("AuthRequired", this.config.ClientAuthConfig?.AuthRequired.ToString());
            result.Properties.Add("Port", this.config.Port.ToString());
            return result;
        }
    }
}

