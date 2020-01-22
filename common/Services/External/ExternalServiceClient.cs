using System;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.External
{
    public class ExternalServiceClient : IExternalServiceClient
    {
        protected readonly string serviceUri;
        protected readonly IExternalRequestHelper _requestHelper;

        public ExternalServiceClient(string serviceUri, IExternalRequestHelper requestHelper)
        {
            this.serviceUri = serviceUri;
            this._requestHelper = requestHelper;
        }

        public virtual async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                StatusServiceModel status = await this._requestHelper.ProcessStatusAsync(this.serviceUri);
                return status.Status;
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, $"Unable to get the status of external service client at {this.serviceUri}/status. {e.Message}");
            }
        }
    }
}