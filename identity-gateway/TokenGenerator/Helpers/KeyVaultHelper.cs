using Microsoft.Azure.KeyVault;
using Microsoft.Azure;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DependencyResolver;

namespace TokenGenerator.Helpers
{
    public static class KeyVaultHelper
    {
        public static KeyVaultClient getKeyVault(IConfiguration _config)
        {
            string AzureServicesAuthConnectionString = "RunAs=App;AppId=" + _config["keyvaultAppId"] + ";TenantId=" +
                    _config["tenantId"] + ";AppKey=" + _config["keyvaultAppKey"] + ";";

            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider(AzureServicesAuthConnectionString);

            return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        }
        public static string getKeyVaultSecretIdentifier(string secret, IConfiguration _config)
        {
            return "https://" + _config["keyvaultName"] + ".vault.azure.net/secrets/" + secret;
        }
    }
}
