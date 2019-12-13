using System.Threading.Tasks;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public interface ITenantContainer
    {
        Task<TenantModel> GetTenantAsync(string tenantGuid);
        Task<CreateTenantModel> CreateTenantAsync(string tenantGuid, string userId);
        Task<DeleteTenantModel> DeleteTenantAsync(string tenantGuid, string userId, bool ensureFullyDeployed = true);
        Task<bool> TenantIsReadyAsync(string tenantGuid);
    }
}