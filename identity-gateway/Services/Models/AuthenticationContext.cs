using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Mmm.Platform.IoT.IdentityGateway.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Models
{
    public class AuthenticationContext : IAuthenticationContext
    {
        private readonly Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext authContext;

        public AuthenticationContext(IServicesConfig _config)
        {
            this.authContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext($"https://login.microsoftonline.com/{_config.AadTenantId}");
        }
        public Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential)
        {
            return authContext.AcquireTokenAsync(resource, clientCredential);
        }
    }
}
