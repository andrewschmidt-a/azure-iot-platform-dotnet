// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.IoTHubManager.Services;
using Mmm.Platform.IoT.IoTHubManager.WebService.v1.Models;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.v1.Controllers
{
    [Route("v1/[controller]")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class DevicePropertiesController : Controller
    {
        private readonly IDeviceProperties deviceProperties;

        public DevicePropertiesController(IDeviceProperties deviceProperties)
        {
            this.deviceProperties = deviceProperties;
        }

        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<DevicePropertiesApiModel> GetAsync()
        {
            return new DevicePropertiesApiModel(await this.deviceProperties.GetListAsync());
        }
    }
}
