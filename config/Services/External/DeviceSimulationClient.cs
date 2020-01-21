using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Config.Services.Helpers;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public class DeviceSimulationClient : IDeviceSimulationClient
    {
        private const int DefaultSimulationId = 1;
        private const string TenantHeader = "ApplicationTenantID";
        private const string TenantId = "TenantID";
        private readonly IHttpClientWrapper httpClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly string serviceUri;

        public DeviceSimulationClient(
            IHttpClientWrapper httpClient,
            AppConfig config,
            IHttpContextAccessor httpContextAccessor)
        {

            this.httpClient = httpClient;

            this.serviceUri = config.ExternalDependencies.DeviceSimulationServiceUrl;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<SimulationApiModel> GetDefaultSimulationAsync()
        {
            SetHttpClientHeaders();
            return await this.httpClient.GetAsync<SimulationApiModel>($"{this.serviceUri}/simulations/{DefaultSimulationId}", $"Simulation {DefaultSimulationId}", true);
        }

        public async Task UpdateSimulationAsync(SimulationApiModel model)
        {
            SetHttpClientHeaders();
            await this.httpClient.PutAsync($"{this.serviceUri}/simulations/{model.Id}", $"Simulation {model.Id}");
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