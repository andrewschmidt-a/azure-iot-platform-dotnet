using Mmm.Platform.IoT.IdentityGateway.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public class UserTenantInput : IUserInput<UserTenantModel>
    {
        public string UserId { get; set; }

        public string Tenant { get; set; }

        public string Roles { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}