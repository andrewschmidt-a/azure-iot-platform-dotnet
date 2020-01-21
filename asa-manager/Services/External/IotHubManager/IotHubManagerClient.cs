using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager
{
    public class IotHubManagerClient : IIotHubManagerClient
    {
        private readonly IExternalRequestHelper requestHelper;
        private readonly string apiUrl;

        public IotHubManagerClient(AppConfig config, IExternalRequestHelper requestHelper)
        {
            this.apiUrl = config.ExternalDependencies.IotHubManagerServiceUrl;
            this.requestHelper = requestHelper;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                StatusServiceModel status = await this.requestHelper.ProcessRequestAsync<StatusServiceModel>(HttpMethod.Get, $"{apiUrl}/status");
                return status.Status;
            }
            catch (Exception)
            {
                return new StatusResultServiceModel(false, "Unable to get the status of the IoT Hub Manager Service. The service may be down or misconfigured.");
            }
        }

        public async Task<DeviceListModel> GetListAsync(IEnumerable<DeviceGroupConditionModel> conditions, string tenantId)
        {
            try
            {
                var query = JsonConvert.SerializeObject(conditions);
                var url = $"{this.apiUrl}/devices?query={query}";
                return await this.requestHelper.ProcessRequestAsync<DeviceListModel>(HttpMethod.Get, url, tenantId);
            }
            catch (Exception e)
            {
                throw new ExternalDependencyException("Unable to get list of devices", e);
            }
        }
    }
}