using System.Threading.Tasks;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.External
{
    public interface IIdentityGatewayClient
    {
        Task<StatusResultServiceModel> StatusAsync();

        Task<IdentityGatewayApiModel> addTenantForUserAsync(string userId, string tenantId, string roles);

        Task<IdentityGatewayApiModel> getTenantForUserAsync(string userId, string tenantId);

        Task<bool> isUserAuthenticated(string userId, string tenantId);

        Task<IdentityGatewayApiSettingModel> addSettingsForUserAsync(string userId, string settingKey, string settingValue);

        Task<IdentityGatewayApiSettingModel> getSettingsForUserAsync(string userId, string settingKey);

        Task<IdentityGatewayApiSettingModel> updateSettingsForUserAsync(string userId, string settingKey, string settingValue);

        Task<IdentityGatewayApiModel> deleteTenantForAllUsersAsync(string tenantId);
    }
}