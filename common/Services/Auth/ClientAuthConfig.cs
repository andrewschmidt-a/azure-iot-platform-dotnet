// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Mmm.Platform.IoT.Common.Services.Auth
{
    public class ClientAuthConfig : IClientAuthConfig
    {
        public string CorsWhitelist { get; set; }
        public bool CorsEnabled => !string.IsNullOrEmpty(this.CorsWhitelist.Trim());

        public bool AuthRequired { get; set; }
        public string AuthType { get; set; }
        public IEnumerable<string> JwtAllowedAlgos { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
        public TimeSpan JwtClockSkew { get; set; }
        public List<JsonWebKey> JwtSecurityKeys { get; set; }
    }
}
