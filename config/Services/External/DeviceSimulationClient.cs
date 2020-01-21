using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.External;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public class DeviceSimulationClient : ExternalServiceClient, IDeviceSimulationClient
    {
        private const int DEFAULT_SIMULATION_ID = 1;

        public DeviceSimulationClient(
            AppConfig config,
            IExternalRequestHelper requestHelper) :
            base(config.ExternalDependencies.DeviceSimulationServiceUrl, requestHelper)
        {
        }
    
        public async Task<SimulationApiModel> GetDefaultSimulationAsync()
        {
            return await this._requestHelper.ProcessRequestAsync<SimulationApiModel>(
                HttpMethod.Get,
                $"{this.serviceUri}/simulations/{DEFAULT_SIMULATION_ID}");
        }

        public async Task UpdateSimulationAsync(SimulationApiModel model)
        {
            await this._requestHelper.ProcessRequestAsync(
                HttpMethod.Put,
                $"{this.serviceUri}/simulations/{model.Id}");
        }
    }
}