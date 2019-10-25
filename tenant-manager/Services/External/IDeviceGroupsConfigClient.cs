using System.Threading.Tasks;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.External
{
    public interface IDeviceGroupsConfigClient : IStatusOperation
    {
        Task<DeviceGroupApiModel> CreateDefaultDeviceGroupAsync(string tenantId);
    }
}