using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups;
using Mmm.Platform.IoT.Common.Services.External;

namespace Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager
{
    public interface IIotHubManagerClient : IExternalServiceClient
    {
        Task<DeviceListModel> GetListAsync(IEnumerable<DeviceGroupConditionModel> conditions, string tenantId);
    }
}