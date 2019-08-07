using Microsoft.Azure.KeyVault;
using Microsoft.Azure;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.KeyVault.Models;

namespace tenant_manager.Helpers
{
    public class KeyVaultHelper : IDisposable
    {
        private IKeyVaultClient client;
        private IConfiguration _config;

        public KeyVaultHelper(IConfiguration _config)
        {
            string keyVaultAppId = _config["Global:AzureActiveDirectory:aadappid"];
            string keyVaultAppKey = _config["KeyVault:aadappsecret"];
            string aadTenantId = _config["Global:AzureActiveDirectory:aadtenantid"];

            string AzureServicesAuthConnectionString = $"RunAs=App;AppId={keyVaultAppId};TenantId={aadTenantId};AppKey={keyVaultAppKey};";

            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider(AzureServicesAuthConnectionString);

            client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            this._config = _config;
        }

        public string getKeyVaultSecretIdentifier(string secret)
        {
            var keyVaultName = this._config["KeyVault:newName"]; // TODO: remove new once app config gets fixed
            return $"https://{keyVaultName}.vault.azure.net/secrets/{secret}";
        }
        public async Task<string> getSecretAsync(string secret)
        {
            return (await client.GetSecretAsync(getKeyVaultSecretIdentifier(secret))).Value;
            
        }
        public void Dispose()
        {

        }
    }
}