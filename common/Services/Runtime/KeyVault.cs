using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.Logging;

namespace Mmm.Platform.IoT.Common.Services.Runtime
{
    public class KeyVault
    {
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        private readonly ILogger _logger;

        // Key Vault Client
        private readonly KeyVaultClient keyVaultClient;

        // Constants
        private const string KEY_VAULT_URI = "https://{0}.vault.azure.net/secrets/{1}";

        public KeyVault() : this(null)
        {
        }

        public KeyVault(ILogger<KeyVault> logger) : this(null, null, null, logger)
        {
        }

        public KeyVault(
            string name,
            string clientId,
            string clientSecret,
            ILogger<KeyVault> logger)
        {
            Name = name;
            ClientId = clientId;
            ClientSecret = clientSecret;
            _logger = logger;
            this.keyVaultClient = new KeyVaultClient(this.GetToken);
        }

        public string GetSecret(string secretKey)
        {
            secretKey = secretKey.Split(':').Last();
            var uri = string.Format(KEY_VAULT_URI, Name, secretKey);

            try
            {
                return this.keyVaultClient.GetSecretAsync(uri).Result.Value;
            }
            catch (Exception)
            {
                _logger?.LogError($"Secret {secretKey} not found in Key Vault.");
                return null;
            }
        }

        //the method that will be provided to the KeyVaultClient
        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(ClientId, ClientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
            {
                _logger?.LogDebug($"Failed to obtain authentication token from key vault.");
                throw new System.InvalidOperationException("Failed to obtain the JWT token");
            }

            return result.AccessToken;
        }
    }
}