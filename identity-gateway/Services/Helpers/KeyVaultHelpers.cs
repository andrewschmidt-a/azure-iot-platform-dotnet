using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Helpers
{
    public class KeyVaultHelpers : IKeyVaultHelpers
    {
        const string KeyVaultAppId = "Global:AzureActiveDirectory:aadappid";
        const string KeyVaultSecret = "Global:AzureActiveDirectory:aadappsecret";
        const string KeyVaultName = "Global:KeyVault:name";
        const string TenantID = "Global:AzureActiveDirectory:aadtenantid";
        const string IdentityGatewayPrivateKey = "IdentityGatewayPrivateKey";

        private IKeyVaultClient client;
        private IConfiguration _config;

        public KeyVaultHelpers(IConfiguration config)
        {
            string AzureServicesAuthConnectionString =
                $"RunAs=App;AppId={config[KeyVaultAppId]};TenantId={config[TenantID]};AppKey={config[KeyVaultSecret]};";

            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider(AzureServicesAuthConnectionString);

            this.client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            this._config = config;
            List<string> requiredKeys = new List<string>
            {
                KeyVaultAppId,
                KeyVaultSecret,
                KeyVaultName,
                TenantID
            };
            if (requiredKeys.Any(key => this._config[key] == null))
            {
                throw new Exception("One of the required Key vault secrets is not configured correctly");
            }
        }

        public string GetKeyVaultSecretIdentifier(string secret)
        {
            return $"https://{ this._config[KeyVaultName]}.vault.azure.net/secrets/{secret}";
        }

        public async Task<string> GetSecretAsync(string secret)
        {
            return (await this.client.GetSecretAsync(GetKeyVaultSecretIdentifier(secret))).Value;
        }

        public async Task<StatusResultServiceModel> PingAsync()
        {
            try
            {
                var value = await this.GetSecretAsync(IdentityGatewayPrivateKey);
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
