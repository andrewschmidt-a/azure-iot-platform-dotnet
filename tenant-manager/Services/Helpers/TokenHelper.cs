using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;

namespace Mmm.Platform.IoT.TenantManager.Services.Helpers
{
    public class TokenHelper : ITokenHelper
    {
        private AppConfig config;

        public TokenHelper(AppConfig config)
        {
            this.config = config;
        }
        
        public async Task<string> GetTokenAsync()
        {
            string keyVaultAppId = config.Global.AzureActiveDirectory.AppId;
            string aadTenantId = config.Global.AzureActiveDirectory.TenantId;
            string keyVaultAppKey = config.Global.AzureActiveDirectory.AppSecret;

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