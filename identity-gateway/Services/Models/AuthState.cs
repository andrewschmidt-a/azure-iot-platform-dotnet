
namespace Mmm.Platform.IoT.IdentityGateway.Services.Models
{
    public class AuthState
    {
        public string ReturnUrl;
        public string State;
        public string Tenant;
        public string Nonce;
        public string ClientId;
        public string Invitation = null;
    }
}
