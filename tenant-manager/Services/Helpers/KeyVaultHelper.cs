using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.KeyVault.Models;
using MMM.Azure.IoTSolutions.TenantManager.Services;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Helpers
{
    public class KeyVaultHelper : IDisposable, IStatusOperation
    {
        private const string GLOBAL_KEY = "Global:";
        private const string GLOBAL_AAD_KEY = GLOBAL_KEY + "AzureActiveDirectory:";
        private const string GLOBAL_KEYVAULT_KEY = GLOBAL_KEY + "KeyVault:";
        private const string KEYVAULT_NAME_KEY = GLOBAL_KEYVAULT_KEY + "name";

        private const string STORAGE_ACCOUNT_CONNECTION_STRING_KEY = "storageAccountConnectionString";

        private IKeyVaultClient client;

        private IConfiguration _config;

        public KeyVaultHelper(IConfiguration _config)
        { 
            string keyVaultAppId = _config[$"{GLOBAL_AAD_KEY}aadappid"];
            string keyVaultAppKey = _config[$"{GLOBAL_AAD_KEY}aadappsecret"];
            string aadTenantId = _config[$"{GLOBAL_AAD_KEY}aadtenantid"];
            
            string AzureServicesAuthConnectionString = $"RunAs=App;AppId={keyVaultAppId};TenantId={aadTenantId};AppKey={keyVaultAppKey};";

            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider(AzureServicesAuthConnectionString);

            this.client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            this._config = _config;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            var value = "";
            try
            {
                value = await this.GetSecretAsync(STORAGE_ACCOUNT_CONNECTION_STRING_KEY);
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, e.Message);
            }

            if (String.IsNullOrEmpty(value))
            {
                // a null value was returned where a value was expected
                return new StatusResultServiceModel(false, "Could not get value from KeyVault");
            }
            else
            {
                // the ping was successful and a value retrieved as expected
                return new StatusResultServiceModel(true, "Alive and well!");
            }
        }

        public string GetKeyVaultSecretIdentifier(string secret)
        {
            var keyVaultName = this._config[KEYVAULT_NAME_KEY]; // TODO: remove new once app config gets fixed
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