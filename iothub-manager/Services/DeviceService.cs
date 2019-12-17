// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Mmm.Platform.IoT.IoTHubManager.Services.Helpers;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Mmm.Platform.IoT.IoTHubManager.Services.Runtime;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public interface IDeviceService
    {
        Task<MethodResultServiceModel> InvokeDeviceMethodAsync(string deviceId, MethodParameterServiceModel parameter);
    }

    public class DeviceService : IDeviceService
    {
        private ServiceClient serviceClient;

        public DeviceService(IServicesConfig config, IHttpContextAccessor httpContextAccessor)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            TenantConnectionHelper tenantHelper = new TenantConnectionHelper(httpContextAccessor, config);

            IoTHubConnectionHelper.CreateUsingHubConnectionString(
                tenantHelper.getIoTHubConnectionString(),
                conn => { this.serviceClient = ServiceClient.CreateFromConnectionString(conn); });
        }

        public async Task<MethodResultServiceModel> InvokeDeviceMethodAsync(string deviceId, MethodParameterServiceModel parameter)
        {
            var result = await this.serviceClient.InvokeDeviceMethodAsync(deviceId, parameter.ToAzureModel());
            return new MethodResultServiceModel(result);
        }
    }
}
