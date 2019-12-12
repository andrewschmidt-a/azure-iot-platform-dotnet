// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;

namespace Mmm.Platform.IoT.Common.Services.Runtime
{
    public class ConfigData : IConfigData
    {
        private IConfiguration configuration;
        private readonly ILogger _logger;

        // Key Vault
        private readonly KeyVault keyVault;

        // Constants
        private const string GLOBAL_KEY = "Global:";
        private const string ALLOWED_ACTION_KEY = "Global:Permissions";

        private const string AAD_KEY = GLOBAL_KEY + "AzureActiveDirectory:"; 
        private const string CLIENT_ID = AAD_KEY + "aadAppId";
        private const string CLIENT_SECRET = AAD_KEY + "aadAppSecret";
        private const string CLIENT_TENANT = AAD_KEY + "aadTenantId";

        private const string KEY_VAULT_NAME = GLOBAL_KEY + "KeyVault:name";

        private const string APP_CONFIGURATION = "PCS_APPLICATION_CONFIGURATION";

        private readonly List<string> appConfigKeys = new List<string>
        {
            "Actions",
            "AsaManagerService",
            "ConfigService",
            "ConfigService:Actions",
            "ExternalDependencies",
            "Global",
            "Global:AzureActiveDirectory",
            "Global:KeyVault",
            "Global:ClientAuth",
            "Global:ClientAuth:JWT",
            "Global:Permissions",
            "Global:Permissions:admin",
            "Global:Permissions:readonly",
            "Global:StorageAccount",
            "IdentityGatewayService",
            "IothubManagerService",
            "IothubManagerService:DevicePropertiesCache",
            "StorageAdapter",
            "TenantManagerService",
            "TelemetryService",
            "TelemetryService:Alarms",
            "TelemetryService:CosmosDb",
            "TelemetryService:Messages",
            "TelemetryService:TimeSeries"
        };

        public ConfigData() : this(null, null)
        {
        }

        public ConfigData(ILogger<ConfigData> logger, KeyVault kv)
        {
            _logger = logger;
            InitializeConfiguration();
            keyVault = kv;
            SetUpKeyVault();
        }

        public string AppConfigurationConnectionString
        {
            get
            {
                return this.GetString(APP_CONFIGURATION);
            }
        }

        public Dictionary<string, List<string>> UserPermissions
        {
            get
            {
                Dictionary<string, List<string>> permissions = new Dictionary<string, List<string>>();
                foreach (var roleSection in this.configuration.GetSection(ALLOWED_ACTION_KEY).GetChildren())
                {
                    permissions.Add(roleSection.Key, roleSection.GetChildren().Select(t => t.Key).ToList());
                }
                return permissions;
            }
        }

        public string AadAppId
        {
            get
            {
                return this.GetString(CLIENT_ID);
            }
        }

        public string AadAppSecret
        {
            get
            {
                return this.GetString(CLIENT_SECRET);
            }
        }

        public string AadTenantId
        {
            get
            {
                return this.GetString(CLIENT_TENANT);
            }
        }

        public string KeyVaultName
        {
            get
            {
                return this.GetString(KEY_VAULT_NAME);
            }
        }

        private void InitializeConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
#if DEBUG
                .AddIniFile("appsettings.ini", optional: true, reloadOnChange: true)
#endif
            .AddEnvironmentVariables();

            var preConfig = configurationBuilder.Build();
            configurationBuilder.Add(new AppConfigurationSource(preConfig[APP_CONFIGURATION], this.appConfigKeys));
            configuration = configurationBuilder.Build();
        }

        public string GetString(string key, string defaultValue = "")
        {
            var value = this.GetSecrets(key, defaultValue);
            this.ReplaceEnvironmentVariables(ref value, defaultValue);
            return value;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = this.GetSecrets(key, defaultValue.ToString()).ToLowerInvariant();

            var knownTrue = new HashSet<string> { "true", "t", "yes", "y", "1", "-1" };
            var knownFalse = new HashSet<string> { "false", "f", "no", "n", "0" };

            if (knownTrue.Contains(value)) return true;
            if (knownFalse.Contains(value)) return false;

            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            try
            {
                return Convert.ToInt32(this.GetSecrets(key, defaultValue.ToString()));
            }
            catch (Exception e)
            {
                throw new InvalidConfigurationException($"Unable to load configuration value for '{key}'", e);
            }
        }

        private void SetUpKeyVault()
        {
            if (keyVault == null)
            {
                return;
            }

            var clientId = this.GetEnvironmentVariable(CLIENT_ID, string.Empty);
            var clientSecret = this.GetEnvironmentVariable(CLIENT_SECRET, string.Empty);
            var keyVaultName = this.GetEnvironmentVariable(KEY_VAULT_NAME, string.Empty);
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(keyVaultName))
            {
                throw new Exception("One of the required key vault keys was not configured correctly.");
            }

            keyVault.ClientId = clientId;
            keyVault.Name = keyVaultName;
            keyVault.ClientSecret = clientSecret;
        }

        private string GetSecrets(string key, string defaultValue = "")
        {
            string value = string.Empty;

            value = this.GetLocalVariables(key, defaultValue);

            // If secrets are not found locally, search in Key-Vault
            if (string.IsNullOrEmpty(value))
            {
                _logger?.LogWarning("Value for secret {key} not found in local env. Trying to get the secret from KeyVault.", key);
                value = this.keyVault?.GetSecret(key);
            }

            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        private string GetSecretsFromKeyVault(string key)
        {
            return this.keyVault?.GetSecret(key);
        }

        private string GetLocalVariables(string key, string defaultValue = "")
        {
            return this.configuration.GetValue(key, defaultValue);
        }

        public string GetEnvironmentVariable(string key, string defaultValue = "")
        {
            var value = this.configuration.GetValue(key, defaultValue);
            this.ReplaceEnvironmentVariables(ref value, defaultValue);
            return value;
        }

        private void ReplaceEnvironmentVariables(ref string value, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(value)) return;

            this.ProcessMandatoryPlaceholders(ref value);

            this.ProcessOptionalPlaceholders(ref value, out bool notFound);

            if (notFound && string.IsNullOrEmpty(value))
            {
                value = defaultValue;
            }
        }

        private void ProcessMandatoryPlaceholders(ref string value)
        {
            // Pattern for mandatory replacements: ${VAR_NAME}
            const string PATTERN = @"\${([a-zA-Z_][a-zA-Z0-9_]*)}";

            // Search
            var keys = (from Match m in Regex.Matches(value, PATTERN)
                        select m.Groups[1].Value).Distinct().ToArray();

            // Replace
            foreach (DictionaryEntry x in Environment.GetEnvironmentVariables())
            {
                if (keys.Contains(x.Key))
                {
                    value = value.Replace("${" + x.Key + "}", x.Value.ToString());
                }
            }

            // Non replaced placeholders cause an exception
            keys = (from Match m in Regex.Matches(value, PATTERN)
                    select m.Groups[1].Value).ToArray();
            if (keys.Length > 0)
            {
                var varsNotFound = keys.Aggregate(", ", (current, k) => current + k);
                _logger?.LogError("Environment variables not found: {environmentVariables}", varsNotFound);
                throw new InvalidConfigurationException("Environment variables not found: " + varsNotFound);
            }
        }

        private void ProcessOptionalPlaceholders(ref string value, out bool notFound)
        {
            notFound = false;

            // Pattern for optional replacements: ${?VAR_NAME}
            const string PATTERN = @"\${\?([a-zA-Z_][a-zA-Z0-9_]*)}";

            // Search
            var keys = (from Match m in Regex.Matches(value, PATTERN)
                        select m.Groups[1].Value).Distinct().ToArray();

            // Replace
            foreach (DictionaryEntry x in Environment.GetEnvironmentVariables())
            {
                if (keys.Contains(x.Key))
                {
                    value = value.Replace("${?" + x.Key + "}", x.Value.ToString());
                }
            }

            // Non replaced placeholders cause an exception
            keys = (from Match m in Regex.Matches(value, PATTERN)
                    select m.Groups[1].Value).ToArray();
            if (keys.Length > 0)
            {
                // Remove placeholders
                value = keys.Aggregate(value, (current, k) => current.Replace("${?" + k + "}", string.Empty));

                var varsNotFound = keys.Aggregate(", ", (current, k) => current + k);
                _logger?.LogWarning("Environment variables not found: {environmentVariables}", varsNotFound);

                notFound = true;
            }
        }
    }
}
