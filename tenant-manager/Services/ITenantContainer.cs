using System.Threading.Tasks;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.Services
{
    public interface ITenantContainer
    {
        Task<CreateTenantModel> CreateTenantAsync(string tenantGuid);
        Task<TenantModel> GetTenantAsync(string tenantGuid);
        Task<DeleteTenantModel> DeleteTenantAsync(string tenantGuid, bool ensureFullyDeployed = true);
        
        Task<bool> TenantIsReadyAsync(string tenantGuid);
    }
}