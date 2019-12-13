using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.TenantManager.Services.Runtime;

namespace Mmm.Platform.IoT.TenantManager.Services.Helpers
{
    public class TokenHelper : ITokenHelper
    {
        private IServicesConfig _config;

        public TokenHelper(IServicesConfig config)
        {
            this._config = config;
        }
        
        public async Task<string> GetTokenAsync()
        {
            string keyVaultAppId = this._config.AzureActiveDirectoryAppId;
            string aadTenantId = this._config.AzureActiveDirectoryTenant;
            string keyVaultAppKey = this._config.AzureActiveDirectoryAppKey;

            // Retrieve a token from Azure AD using the application id and password.
            try
            {
                var authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", aadTenantId));
                var credential = new ClientCredential(keyVaultAppId, keyVaultAppKey);
                AuthenticationResult token = await authContext.AcquireTokenAsync("https://management.core.windows.net/", credential);

                if (token == null)
                {
                    throw new NoAuthorizationException("The authentication context returned a null value while aquiring the authentication token.");
                }

                return token.AccessToken;
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve authentication access token in GetServicePrincipalToken().", e);
            }
        }
    }
}