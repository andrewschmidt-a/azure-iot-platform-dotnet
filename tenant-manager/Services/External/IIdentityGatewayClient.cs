using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services.External
{
    public interface IIdentityGatewayClient : IExternalServiceClient
    {
        Task<IdentityGatewayApiModel> addTenantForUserAsync(string userId, string tenantId, string roles);

        Task<IdentityGatewayApiModel> getTenantForUserAsync(string userId, string tenantId);

        Task<IdentityGatewayApiSettingModel> addSettingsForUserAsync(string userId, string settingKey, string settingValue);

        Task<IdentityGatewayApiSettingModel> getSettingsForUserAsync(string userId, string settingKey);

        Task<IdentityGatewayApiSettingModel> updateSettingsForUserAsync(string userId, string settingKey, string settingValue);

        Task<IdentityGatewayApiModel> deleteTenantForAllUsersAsync(string tenantId);
    }
}