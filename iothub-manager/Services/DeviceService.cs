// <copyright file="DeviceService.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Mmm.Iot.Common.Services.Config;
using Mmm.Iot.IoTHubManager.Services.Helpers;
using Mmm.Iot.IoTHubManager.Services.Models;

namespace Mmm.Iot.IoTHubManager.Services
{
    public class DeviceService : IDeviceService
    {
        private ServiceClient serviceClient;

        public DeviceService(ITenantConnectionHelper tenantConnectionHelper)
        {
            IoTHubConnectionHelper.CreateUsingHubConnectionString(
                tenantConnectionHelper.GetIotHubConnectionString(),
                conn => { this.serviceClient = ServiceClient.CreateFromConnectionString(conn); });
        }

        public async Task<MethodResultServiceModel> InvokeDeviceMethodAsync(string deviceId, MethodParameterServiceModel parameter)
        {
            var result = await this.serviceClient.InvokeDeviceMethodAsync(deviceId, parameter.ToAzureModel());
            return new MethodResultServiceModel(result);
        }
    }
}