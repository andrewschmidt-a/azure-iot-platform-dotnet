
namespace IdentityGateway.Services.Models
{
    public class AuthState
    {
        public string returnUrl;
        public string state;
        public string tenant;
        public string nonce;
    }
}
