using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Helpers
{
    public class TokenHelper
    {
        private IServicesConfig _config;

        public TokenHelper(IServicesConfig _config)
        {
            this._config = _config;
        }

        public string GetServicePrincipleToken()
        {
            string keyVaultAppId = this._config.AzureActiveDirectoryAppId;
            string aadTenantId = this._config.AzureActiveDirectoryTenant;
            string keyVaultAppKey = this._config.AzureActiveDirectoryAppKey;

            // Retrieve a token from Azure AD using the application id and password.
            try
            {
                var authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", aadTenantId));
                var credential = new ClientCredential(keyVaultAppId, keyVaultAppKey);
                AuthenticationResult token = authContext.AcquireTokenAsync("https://management.core.windows.net/", credential).Result;

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