using Mmm.Platform.IoT.IdentityGateway.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public class UserSettingsInput : IUserInput<UserSettingsModel>
    {
        public string UserId { get; set; }

        public string SettingKey { get; set; }

        public string Value { get; set; }
    }
}