
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services.External
{
    public class AzureB2cClient : IAzureB2cClient
    {
        public string serviceUri;

        private readonly IExternalRequestHelper _requestHelper;

        public AzureB2cClient(
            AppConfig config,
            IExternalRequestHelper requestHelper)
        {
            this.serviceUri = config.Global.AzureB2cBaseUri;
            this._requestHelper = requestHelper;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                var response = await this._requestHelper.ProcessRequestAsync(HttpMethod.Get, this.serviceUri);
                if (response.IsSuccessStatusCode)
                {
                    return new StatusResultServiceModel(response.IsSuccessStatusCode, "Alive and well!");
                }
                else
                {
                    return new StatusResultServiceModel(false, $"AzureB2C status check failed with code {response.StatusCode}.");
                }
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, e.Message);
            }
        }
    }
}