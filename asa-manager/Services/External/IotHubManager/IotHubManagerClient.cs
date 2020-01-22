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
using Mmm.Platform.IoT.Common.Services.External;

namespace Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager
{
    public class IotHubManagerClient : ExternalServiceClient, IIotHubManagerClient
    {
        public IotHubManagerClient(
            AppConfig config,
            IExternalRequestHelper requestHelper) :
            base(config.ExternalDependencies.IotHubManagerServiceUrl, requestHelper)
        {
        }

        public async Task<DeviceListModel> GetListAsync(IEnumerable<DeviceGroupConditionModel> conditions, string tenantId)
        {
            try
            {
                var query = JsonConvert.SerializeObject(conditions);
                var url = $"{this.serviceUri}/devices?query={query}";
                return await this._requestHelper.ProcessRequestAsync<DeviceListModel>(HttpMethod.Get, url, tenantId);
            }
            catch (Exception e)
            {
                throw new ExternalDependencyException("Unable to get list of devices", e);
            }
        }
    }
}