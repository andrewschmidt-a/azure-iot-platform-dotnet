using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Models
{
    public interface IAuthenticationContext
    {
        Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential);
    }
}
