using System.Threading.Tasks;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public interface ITenantContainer
    {
        Task<CreateTenantModel> CreateTenantAsync(string tenantGuid);
        Task<TenantModel> GetTenantAsync(string tenantGuid);
        Task<DeleteTenantModel> DeleteTenantAsync(string tenantGuid, bool ensureFullyDeployed = true);
        
        Task<bool> TenantIsReadyAsync(string tenantGuid);
    }
}