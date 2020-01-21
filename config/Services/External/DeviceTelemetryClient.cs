// <copyright file="DeviceTelemetryClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.Helpers;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public class DeviceTelemetryClient : ExternalServiceClient, IDeviceTelemetryClient
    {
        public DeviceTelemetryClient(AppConfig config, IExternalRequestHelper requestHelper)
            : base(config.ExternalDependencies.TelemetryServiceUrl, requestHelper)
        {
        }

        public async Task UpdateRuleAsync(RuleApiModel rule, string etag)
        {
            rule.ETag = etag;
            await this.RequestHelper.ProcessRequestAsync(HttpMethod.Put, $"{this.ServiceUri}/rules/{rule.Id}", rule);
        }
    }
}