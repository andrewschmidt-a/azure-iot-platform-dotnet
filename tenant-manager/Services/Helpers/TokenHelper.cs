using System;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MMM.Azure.IoTSolutions.TenantManager.Services.Exceptions;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Helpers
{
    public class TokenHelper
    {
        private const string GLOBAL_KEY = "Global:";
        private const string GLOBAL_AAD_KEY = GLOBAL_KEY + "AzureActiveDirectory:";

        private IConfiguration _config;

        public TokenHelper(IConfiguration _config)
        {
            this._config = _config;
        }
        
        public string GetServicePrincipleToken()
        {
            string keyVaultAppId = _config[$"{GLOBAL_AAD_KEY}aadappid"];
            string keyVaultAppKey = _config[$"{GLOBAL_AAD_KEY}aadappsecret"];
            string aadTenantId = _config[$"{GLOBAL_AAD_KEY}aadtenantid"];
            
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