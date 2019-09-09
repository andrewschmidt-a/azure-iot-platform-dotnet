using Microsoft.Azure.KeyVault;
using Microsoft.Azure;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DependencyResolver;
using Microsoft.Azure.KeyVault.Models;
using IdentityGateway.Services.Models;

namespace IdentityGateway.Services.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyVaultHelper : IDisposable
    {
        const string KeyVaultAppId = "KeyVault:aadappid";
        const string KeyVaultSecret = "KeyVault:aadappsecret";
        const string KeyVaultName = "KeyVault:name";
        const string TenantID = "Global:AzureActiveDirectory:aadtenantid";
        const string IdentityGatewayPrivateKey = "IdentityGatewayPrivateKey";

        private IKeyVaultClient client;
        private IConfiguration _config;
        
        public KeyVaultHelper(IConfiguration config)
        {
            string AzureServicesAuthConnectionString =
                $"RunAs=App;AppId={config[KeyVaultAppId]};TenantId={config[TenantID]};AppKey={config[KeyVaultSecret]};";

            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider(AzureServicesAuthConnectionString);

            this.client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            this._config = config;

        }
        
        public string getKeyVaultSecretIdentifier(string secret)
        {
            return $"https://{ this._config[KeyVaultName]}.vault.azure.net/secrets/{secret}";
        }
        
        public async Task<string> getSecretAsync(string secret)
        {
            return (await this.client.GetSecretAsync(getKeyVaultSecretIdentifier(secret))).Value;
        }

        public async Task<StatusResultServiceModel> PingAsync()
        {
            try
            {
                var value = await this.getSecretAsync(IdentityGatewayPrivateKey);
                if (value != null && value != "")
                {
                    return new StatusResultServiceModel(true, "Alive and well!");
                }
            }
            catch (Exception E)
            {
                return new StatusResultServiceModel(false, E.Message);
            }
            return new StatusResultServiceModel(false, "Could not get value from KeyVault");
        }

        public void Dispose()
        {

        }
    }
}
