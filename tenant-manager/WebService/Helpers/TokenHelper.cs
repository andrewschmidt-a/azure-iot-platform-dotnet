using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Exceptions;
using ILogger = Microsoft.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
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