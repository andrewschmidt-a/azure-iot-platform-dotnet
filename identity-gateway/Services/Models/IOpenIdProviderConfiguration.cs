using System.Collections.Generic;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Models
{
    public interface IOpenIdProviderConfiguration
    {
        string issuer { get; }
        string jwks_uri { get; }
        string authorization_endpoint { get; }
        string end_session_endpoint { get; }
        IEnumerable<string> scopes_supported { get; }
        IEnumerable<string> claims_supported { get; }
        IEnumerable<string> grant_types_supported { get; }
        IEnumerable<string> response_types_supported { get; }
        IEnumerable<string> response_modes_supported { get; }
    }
}