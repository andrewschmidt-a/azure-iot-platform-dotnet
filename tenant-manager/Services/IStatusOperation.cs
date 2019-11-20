using System.Threading.Tasks;
using Mmm.Platform.IoT.TenantManager.Services.Models;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public interface IStatusOperation
    {
        Task<StatusResultServiceModel> StatusAsync();
    }
}