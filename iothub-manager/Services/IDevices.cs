using System.Threading.Tasks;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.Common.Services;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public interface IDevices : IStatusOperation
    {
        Task<DeviceServiceListModel> GetListAsync(string query, string continuationToken);
        Task<DeviceTwinName> GetDeviceTwinNamesAsync();
        Task<DeviceServiceModel> GetAsync(string id);
        Task<DeviceServiceModel> CreateAsync(DeviceServiceModel toServiceModel);
        Task<DeviceServiceModel> CreateOrUpdateAsync(DeviceServiceModel toServiceModel, DevicePropertyDelegate devicePropertyDelegate);
        Task DeleteAsync(string id);
        Task<TwinServiceModel> GetModuleTwinAsync(string deviceId, string moduleId);
        Task<TwinServiceListModel> GetModuleTwinsByQueryAsync(string query, string continuationToken);
    }
}
