// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Microsoft.Azure.IoTSolutions.Auth;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
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
            
            
            string tenantId = this._httpContextAccessor.HttpContext.Request.GetTenant();
            this.httpClient.SetHeaders(new Dictionary<string, string> {{TENANT_HEADER, tenantId}});
        }

        public async Task<SimulationApiModel> GetDefaultSimulationAsync()
        {
            return await this.httpClient.GetAsync<SimulationApiModel>($"{this.serviceUri}/simulations/{DEFAULT_SIMULATION_ID}", $"Simulation {DEFAULT_SIMULATION_ID}", true);
        }

        public async Task UpdateSimulationAsync(SimulationApiModel model)
        {
            await this.httpClient.PutAsync($"{this.serviceUri}/simulations/{model.Id}", $"Simulation {model.Id}");
        }
    }
}