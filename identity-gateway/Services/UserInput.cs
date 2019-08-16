using IdentityGateway.Services.Models;

namespace IdentityGateway.Services
{
    public interface IUserInput<TModel>
    {
        string userId { get; set; }
    }

    public class UserTenantInput : IUserInput<UserTenantModel>
    {
        // interface members
        public string userId { get; set; }

        public string tenant;
        public string roles;
    }

    public class UserSettingsInput : IUserInput<UserSettingsModel>
    {
        // interface members
        public string userId { get; set; }

        public string settingKey;
        public string value;
    }
}