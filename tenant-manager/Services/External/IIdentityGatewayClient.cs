using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Models;

namespace Microsoft.Azure.IoTSolutions.TenantManager.Services.External
{
    public interface IIdentityGatewayClient
    {
        Task<IdentityGatewayApiModel> addUserToTenantAsync(string userId, string tenantId, string roles);

        Task<IdentityGatewayApiModel> getTenantForUserAsync(string userId, string tenantId);

        Task<bool> isUserAuthenticated(string userId, string tenantId);

        Task<IdentityGatewayApiSettingModel> addSettingsForUserAsync(string userId, string settingKey, string settingValue);

        Task<IdentityGatewayApiSettingModel> getSettingsForUserAsync(string userId, string settingKey);
    }
}