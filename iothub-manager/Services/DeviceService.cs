using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.IoTHubManager.Services.Helpers;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;

namespace Mmm.Platform.IoT.IoTHubManager.Services
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