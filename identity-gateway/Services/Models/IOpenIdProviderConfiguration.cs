using System.Collections.Generic;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Models
{
    public interface IOpenIdProviderConfiguration
    {
        string Issuer { get; }
        string JwksUri { get; }
        string AuthorizationEndpoint { get; }
        string EndSessionEndpoint { get; }
        IEnumerable<string> ScopesSupported { get; }
        IEnumerable<string> ClaimsSupported { get; }
        IEnumerable<string> GrantTypesSupported { get; }
        IEnumerable<string> ResponseTypesSupported { get; }
        IEnumerable<string> ResponseModesSupported { get; }
    }
}