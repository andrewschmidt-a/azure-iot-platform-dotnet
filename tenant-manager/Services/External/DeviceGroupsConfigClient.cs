using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.TenantManager.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.TenantManager.Services.External
{
    public class DeviceGroupsConfigClient : IDeviceGroupsConfigClient
    {
        private readonly IExternalRequestHelper _requestHelper;
        private readonly string serviceUri;

        public DeviceGroupsConfigClient(AppConfig config, IExternalRequestHelper requestHelper)
        {
            this.serviceUri = config.ExternalDependencies.ConfigServiceUrl;
            this._requestHelper = requestHelper;
        }

        public string RequestUrl(string path)
        {
            return $"{this.serviceUri}/{path}";
        }

        /// <summary>
        /// Ping the DeviceGroups for its status
        /// </summary>
        /// <returns></returns>
        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                string url = this.RequestUrl("status/");
                var result = await this._requestHelper.ProcessRequestAsync<StatusServiceModel>(HttpMethod.Get, url);
                if (result == null || result.Status == null || !result.Status.IsHealthy)
                {
                    // bad status
                    return new StatusResultServiceModel(false, result.Status.Message);
                }
                else
                {
                    return new StatusResultServiceModel(true, "Alive and well!");
                }
            }
            catch (JsonReaderException)
            {
                return new StatusResultServiceModel(false, $"Unable to read the response from the DeviceGroups Status. The service may be down.");
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, $"Unable to get DeviceGroups Status: {e.Message}");
            }
        }

        public async Task<DeviceGroupApiModel> CreateDefaultDeviceGroupAsync(string tenantId)
        {
            DeviceGroupApiModel defaultGroup = new DeviceGroupApiModel
            {
                DisplayName = "Default",
                Conditions = new List<DeviceGroupConditionModel>()
            };
            string url = this.RequestUrl("devicegroups/");
            return await this._requestHelper.ProcessRequestAsync(HttpMethod.Post, url, defaultGroup, tenantId);
        }
    }
}