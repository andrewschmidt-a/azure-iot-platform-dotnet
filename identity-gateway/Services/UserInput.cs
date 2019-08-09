using IdentityGateway.Services.Models;

namespace IdentityGateway.Services
{
    public interface IUserTenantInput<TModel>
    {
        string userId { get; set; }
    }

    public class UserTenantInput : IUserTenantInput<UserTenantModel>
    {
        // interface members
        public string userId { get; set; }

        public string roles;
    }

    public class UserSettingsInput : IUserTenantInput<UserSettingsModel>
    {
        // interface members
        public string userId { get; set; }

        public string settingKey;
        public string value;
    }
}