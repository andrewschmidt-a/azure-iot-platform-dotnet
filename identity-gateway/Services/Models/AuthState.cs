namespace Mmm.Platform.IoT.IdentityGateway.Services.Models
{
    public class AuthState
    {
        public string ReturnUrl { get; set; }

        public string State { get; set; }

        public string Tenant { get; set; }

        public string Nonce { get; set; }

        public string ClientId { get; set; }

        public string Invitation { get; set; }
    }
}
