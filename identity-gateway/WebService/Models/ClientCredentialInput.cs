namespace Mmm.Platform.IoT.IdentityGateway.WebService.Models
{
    public class ClientCredentialInput
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Scope { get; set; }
    }
}