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
using tenant_manager.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace tenant_manager.Helpers
{
    public class TokenHelper
    {
        private IConfiguration _config;

        public TokenHelper(IConfiguration _config)
        {
            this._config = _config;
        }
        
        public string GetServicePrincipleToken()
        {
            /* Returns a token  */            
            string tenantId = this._config["Global:AzureActiveDirectory:aadtenantid"];
            string applicationId = this._config["Global:AzureActiveDirectory:aadappid"];
            string applicationSecret = this._config["KeyVault:aadappsecret"];
            
            // Retrieve a token from Azure AD using the application id and password.
            var authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", tenantId));
            var credential = new ClientCredential(applicationId, applicationSecret);
            AuthenticationResult token = authContext.AcquireTokenAsync("https://management.core.windows.net/", credential).Result;

            if (token == null)
            {
                Console.WriteLine("Failed to obtain the token");
                return "";
            }

            return token.AccessToken;;
        }
    }
}