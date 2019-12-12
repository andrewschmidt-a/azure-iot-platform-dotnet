using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.External.AsaManager
{
    public class AsaManagerClient : IAsaManagerClient
    {
        private readonly string serviceUrl;
        private readonly IExternalRequestHelper _requestHelper;

        public AsaManagerClient(IAsaManagerClientConfig config, IExternalRequestHelper requestHelper)
        {
            this.serviceUrl = config.AsaManagerApiUrl;
            this._requestHelper = requestHelper;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            string url = $"{this.serviceUrl}/status";
            StatusServiceModel status = await this._requestHelper.ProcessRequestAsync<StatusServiceModel>(HttpMethod.Get, url);
            return status.Status;
        }

        public async Task<BeginConversionApiModel> BeginConversionAsync(string entity)
        {
            string url = $"{this.serviceUrl}/{entity}";
            return await this._requestHelper.ProcessRequestAsync<BeginConversionApiModel>(HttpMethod.Post, url);
        }
    }
}
