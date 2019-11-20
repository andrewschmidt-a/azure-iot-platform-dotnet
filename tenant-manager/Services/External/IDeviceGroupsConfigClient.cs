using System.Threading.Tasks;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services.External
{
    public interface IDeviceGroupsConfigClient : IStatusOperation
    {
        Task<DeviceGroupApiModel> CreateDefaultDeviceGroupAsync(string tenantId);
    }
}