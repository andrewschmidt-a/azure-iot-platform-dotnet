using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.Common.Services.Helpers;
using System.Net.Http;
using Mmm.Platform.IoT.Common.Services.External;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public class DeviceTelemetryClient : ExternalServiceClient, IDeviceTelemetryClient
    {
        public DeviceTelemetryClient(
            AppConfig config,
            IExternalRequestHelper requestHelper) :
            base(config.ExternalDependencies.TelemetryServiceUrl, requestHelper)
        {
        }

        public async Task UpdateRuleAsync(RuleApiModel rule, string etag)
        {
            rule.ETag = etag;
            await this._requestHelper.ProcessRequestAsync(HttpMethod.Put, $"{this.serviceUri}/rules/{rule.Id}", rule);
        }
    }
}
