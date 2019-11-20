// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Config.Services.Helpers;
using Mmm.Platform.IoT.Config.Services.Runtime;
using Mmm.Platform.IoT.Common.AuthUtils;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public interface IDeviceSimulationClient
    {
        Task<SimulationApiModel> GetDefaultSimulationAsync();
        Task UpdateSimulationAsync(SimulationApiModel model);
    }

    public class DeviceSimulationClient : IDeviceSimulationClient
    {
        private const int DEFAULT_SIMULATION_ID = 1;
        private readonly IHttpClientWrapper httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string serviceUri;

        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string TENANT_ID = "TenantID";
        public DeviceSimulationClient(
            IHttpClientWrapper httpClient,
            IServicesConfig config,
            IHttpContextAccessor httpContextAccessor)
        {

            this.httpClient = httpClient;

            this.serviceUri = config.DeviceSimulationApiUrl;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Sets the Tenant ID in the http headers
        /// </summary>
        private void SetHttpClientHeaders()
        {
            if (this._httpContextAccessor != null && this.httpClient != null)
            {
                string tenantId = this._httpContextAccessor.HttpContext.Request.GetTenant();
                this.httpClient.SetHeaders(new Dictionary<string, string> { { TENANT_HEADER, tenantId } });
            }
        }
        public async Task<SimulationApiModel> GetDefaultSimulationAsync()
        {
            SetHttpClientHeaders();
            return await this.httpClient.GetAsync<SimulationApiModel>($"{this.serviceUri}/simulations/{DEFAULT_SIMULATION_ID}", $"Simulation {DEFAULT_SIMULATION_ID}", true);
        }

        public async Task UpdateSimulationAsync(SimulationApiModel model)
        {
            SetHttpClientHeaders();
            await this.httpClient.PutAsync($"{this.serviceUri}/simulations/{model.Id}", $"Simulation {model.Id}");
        }
    }
}