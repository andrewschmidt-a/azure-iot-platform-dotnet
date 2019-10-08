using System.Threading.Tasks;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.Services
{
    public interface IStatusOperation
    {
        Task<StatusResultServiceModel> StatusAsync();
    }
}