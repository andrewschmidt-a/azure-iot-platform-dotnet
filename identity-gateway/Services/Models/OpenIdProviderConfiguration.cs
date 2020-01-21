using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Models
{
    public class OpenIdProviderConfiguration : IOpenIdProviderConfiguration
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly string host;

        public OpenIdProviderConfiguration()
        {
        }

        public OpenIdProviderConfiguration(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            string forwardedFor = null;
            if (httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].Count > 0)
            {
                forwardedFor = httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            // checks for http vs https using _httpContext.Request.IsHttps and creates the url accordingly
            host = forwardedFor ?? $"http{(httpContextAccessor.HttpContext.Request.IsHttps ? "s" : string.Empty)}://{httpContextAccessor.HttpContext.Request.Host.ToString()}";
        }

        public virtual string Issuer => host;
        public virtual string JwksUri => host + "/.well-known/openid-configuration/jwks";
        public virtual string AuthorizationEndpoint => host + "/connect/authorize";
        public virtual string EndSessionEndpoint => host + "/connect/logout";
        public virtual IEnumerable<string> ScopesSupported => new List<string> { "openid", "profile" };
        public virtual IEnumerable<string> ClaimsSupported => new List<string> { "sub", "name", "tenant", "role" };
        public virtual IEnumerable<string> GrantTypesSupported => new List<string> { "implicit" };
        public virtual IEnumerable<string> ResponseTypesSupported => new List<string> { "token", "id_token" };
        public virtual IEnumerable<string> ResponseModesSupported => new List<string> { "query" };
    }
}
