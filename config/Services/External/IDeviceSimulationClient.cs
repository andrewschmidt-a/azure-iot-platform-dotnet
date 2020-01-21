using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public interface IDeviceSimulationClient
    {
        Task<SimulationApiModel> GetDefaultSimulationAsync();

        Task UpdateSimulationAsync(SimulationApiModel model);
    }
}