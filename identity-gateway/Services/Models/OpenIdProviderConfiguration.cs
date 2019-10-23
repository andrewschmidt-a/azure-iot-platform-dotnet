using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace IdentityGateway.Services.Models
{
    public class OpenIdProviderConfiguration : IOpenIdProviderConfiguration
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _host;

        public OpenIdProviderConfiguration() { }

        public OpenIdProviderConfiguration(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            string forwardedFor = null;
            if (_httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].Count > 0)
            {
                forwardedFor = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            // checks for http vs https using _httpContext.Request.IsHttps and creates the url accordingly
            _host = forwardedFor ?? $"http{(_httpContextAccessor.HttpContext.Request.IsHttps ? "s" : "")}://{_httpContextAccessor.HttpContext.Request.Host.ToString()}";
        }

        public virtual string issuer => _host;
        public virtual string jwks_uri => _host + "/.well-known/openid-configuration/jwks";
        public virtual string authorization_endpoint => _host + "/connect/authorize";
        public virtual string end_session_endpoint => _host + "/connect/logout";
        public virtual IEnumerable<string> scopes_supported => new List<string> { "openid", "profile" };
        public virtual IEnumerable<string> claims_supported => new List<string> { "sub", "name", "tenant", "role" };
        public virtual IEnumerable<string> grant_types_supported => new List<string> { "implicit" };
        public virtual IEnumerable<string> response_types_supported => new List<string> { "token", "id_token" };
        public virtual IEnumerable<string> response_modes_supported => new List<string> { "query" };
    }
}
