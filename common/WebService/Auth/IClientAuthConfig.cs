using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Mmm.Platform.IoT.Common.WebService.Auth
{
    public interface IClientAuthConfig
    {
        // CORS whitelist, in form { "origins": [], "methods": [], "headers": [] }
        // Defaults to empty, meaning No CORS.
        string CorsWhitelist { get; set; }

        // Whether CORS support is enabled
        // Default: false
        bool CorsEnabled { get; }

        // Whether the authentication and authorization is required or optional.
        // Default: true
        bool AuthRequired { get; set; }

        // Auth type: currently supports only "JWT"
        // Default: JWT
        string AuthType { get; set; }

        // The list of allowed signing algoritms
        // Default: RS256, RS384, RS512
        IEnumerable<string> JwtAllowedAlgos { get; set; }

        // The trusted issuer
        string JwtIssuer { get; set; }

        // The required audience
        string JwtAudience { get; set; }

        // Clock skew allowed when validating tokens expiration
        // Default: 2 minutes
        TimeSpan JwtClockSkew { get; set; }
        List<JsonWebKey> JwtSecurityKeys { get; set; }
    }
}