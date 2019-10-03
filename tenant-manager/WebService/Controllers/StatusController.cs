// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MMM.Azure.IoTSolutions.TenantManager.Services;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Filters;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Runtime;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Controllers
{
    [Route("api/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : ControllerBase
    {
        private readonly IConfig config;
        private readonly IStatusService statusService;

        public StatusController(IConfig config, IStatusService statusService)
        {
            this.config = config;
            this.statusService = statusService;
        }

        public async Task<StatusModel> GetAsync()
        {
            try
            {
                var result = new StatusModel(await this.statusService.GetStatusAsync());
                result.Properties.Add("Port", this.config.Port.ToString());
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while attempting to get the service status", e);
            }
        }
    }
}
