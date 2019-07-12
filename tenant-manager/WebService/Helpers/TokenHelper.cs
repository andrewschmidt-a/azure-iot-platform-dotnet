using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using tenant_manager.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace tenant_manager.Helpers
{
    public class TokenHelper
    {
        public static string GetServicePrincipleToken()
        {
            /* Returns a token  */
            string tenantId = "facac3c4-e2a5-4257-af76-205c8a821ddb";
            string applicationId = "95d3c662-23ea-4e2d-8d3d-ea2448706934";
            string applicationSecret = "YWIwYzhkYjktNjFmZi00MTI5LWE0YjAtNjAyZGNmNWFlNzVk=";
            
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