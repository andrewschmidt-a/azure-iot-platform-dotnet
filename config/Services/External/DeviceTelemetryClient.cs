// <copyright file="DeviceTelemetryClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Config.Services.Helpers;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public class DeviceTelemetryClient : IDeviceTelemetryClient
    {
        private const string TenantHeader = "ApplicationTenantID";
        private const string TenantId = "TenantID";
        private readonly IHttpClientWrapper httpClient;
        private readonly string serviceUri;

        private readonly IHttpContextAccessor httpContextAccessor;

        public DeviceTelemetryClient(
            IHttpClientWrapper httpClient,
            AppConfig config,
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpClient = httpClient;
            this.serviceUri = config.ExternalDependencies.TelemetryServiceUrl;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task UpdateRuleAsync(RuleApiModel rule, string etag)
        {
            SetHttpClientHeaders();
            rule.ETag = etag;

            await this.httpClient.PutAsync($"{this.serviceUri}/rules/{rule.Id}", $"Rule {rule.Id}", rule);
        }

        private void SetHttpClientHeaders()
        {
            if (this.httpContextAccessor != null && this.httpClient != null)
            {
                string tenantId = this.httpContextAccessor.HttpContext.Request.GetTenant();
                this.httpClient.SetHeaders(new Dictionary<string, string> { { TenantHeader, tenantId } });
            }
        }
    }
}