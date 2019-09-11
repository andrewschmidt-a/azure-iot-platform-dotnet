namespace Microsoft.Azure.IoTSolutions.TenantManager.Services.External
{
    public interface IIdentityGatewayClient
    {
        void addUserToTenantAsync(string userId, string tenantId, string roles);
    }
}