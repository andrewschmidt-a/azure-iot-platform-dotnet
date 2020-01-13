using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public interface IDeviceProperties
    {
        Task<List<string>> GetListAsync();

        Task<DevicePropertyServiceModel> UpdateListAsync(
            DevicePropertyServiceModel devicePropertyServiceModel);

        Task<bool> TryRecreateListAsync(bool force = false);
    }
}
