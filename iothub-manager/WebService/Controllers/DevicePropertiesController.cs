// <copyright file="DevicePropertiesController.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.IoTHubManager.Services;
using Mmm.Platform.IoT.IoTHubManager.WebService.Models;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Controllers
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