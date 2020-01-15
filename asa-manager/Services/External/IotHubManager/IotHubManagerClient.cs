using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups;
using Newtonsoft.Json;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager
{
    public class IotHubManagerClient : IIotHubManagerClient
    {
        private readonly IExternalRequestHelper _requestHelper;
        private readonly string apiUrl;

        public IotHubManagerClient(AppConfig config, IExternalRequestHelper requestHelper)
        {
            this.apiUrl = config.ExternalDependencies.IotHubManagerServiceUrl;
            this._requestHelper = requestHelper;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                StatusServiceModel status = await this._requestHelper.ProcessRequestAsync<StatusServiceModel>(HttpMethod.Get, $"{apiUrl}/status");
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
                return await this._requestHelper.ProcessRequestAsync<DeviceListModel>(HttpMethod.Get, url, tenantId);
            }
            catch (Exception e)
            {
                throw new ExternalDependencyException("Unable to get list of devices", e);
            }
        }
    }
}