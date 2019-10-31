using IdentityGateway.Services.Models;

namespace IdentityGateway.Services
{
    public class UserSettingsInput : IUserInput<UserSettingsModel>
    {
        public string UserId { get; set; }
        public string SettingKey { get; set; }
        public string Value { get; set; }
    }
}