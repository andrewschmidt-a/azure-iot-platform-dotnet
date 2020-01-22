using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.External.AsaManager
{
    public class AsaManagerClient : ExternalServiceClient, IAsaManagerClient
    {
        public AsaManagerClient(
            AppConfig config,
            IExternalRequestHelper requestHelper) :
            base(config.ExternalDependencies.AsaManagerServiceUrl, requestHelper)
        {
        }

        public async Task<BeginConversionApiModel> BeginConversionAsync(string entity)
        {
            string url = $"{this.serviceUri}/{entity}";
            return await this._requestHelper.ProcessRequestAsync<BeginConversionApiModel>(HttpMethod.Post, url);
        }
    }
}
