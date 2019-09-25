using Microsoft.Azure.KeyVault;
using Microsoft.Azure;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Exceptions;
using ILogger = Microsoft.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
{
    public class KeyVaultHelper : IDisposable
    {
        private const string GLOBAL_KEY = "Global:";
        private const string GLOBAL_AAD_KEY = GLOBAL_KEY + "AzureActiveDirectory:";
        private const string GLOBAL_KEYVAULT_KEY = GLOBAL_KEY + "KeyVault:";

        private IKeyVaultClient client;

        private IConfiguration _config;

        public KeyVaultHelper(IConfiguration _config)
        { 
            string keyVaultAppId = _config[$"{GLOBAL_AAD_KEY}aadappid"];
            string keyVaultAppKey = _config[$"{GLOBAL_AAD_KEY}aadappsecret"];
            string aadTenantId = _config[$"{GLOBAL_AAD_KEY}aadtenantid"];
            
            string AzureServicesAuthConnectionString = $"RunAs=App;AppId={keyVaultAppId};TenantId={aadTenantId};AppKey={keyVaultAppKey};";

            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider(AzureServicesAuthConnectionString);

            client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            this._config = _config;
        }

        public string GetKeyVaultSecretIdentifier(string secret)
        {
            var keyVaultName = this._config[$"{GLOBAL_KEYVAULT_KEY}name"]; // TODO: remove new once app config gets fixed
            return $"https://{keyVaultName}.vault.azure.net/secrets/{secret}";
        }

        public async Task<string> GetSecretAsync(string secret)
        {
            try
            {
                SecretBundle secretBundle = await this.client.GetSecretAsync(GetKeyVaultSecretIdentifier(secret));
                if (secretBundle == null)
                {
                    throw new NullReferenceException("The SecretBundle returned from keyVault was null. A value could not be returned for the requested secret");
                }
                return secretBundle.Value;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to retrieve secret {secret} from KeyVault", e);
            }
        }

        public void Dispose()
        {

        }
    }
}