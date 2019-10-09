// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MMM.Azure.IoTSolutions.TenantManager.Services.Diagnostics;
using MMM.Azure.IoTSolutions.TenantManager.Services.Exceptions;
using MMM.Azure.IoTSolutions.TenantManager.AppConfiguration;
using Microsoft.Extensions.Configuration;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Runtime
{
    public interface IConfigData
    {
        Dictionary<string, List<string>> GetUserPermissions();
        string GetString(string key, string defaultValue = "");
        bool GetBool(string key, bool defaultValue = false);
        int GetInt(string key, int defaultValue = 0);
    }

    public class ConfigData : IConfigData
    {
        private readonly IConfigurationRoot configuration;
        private readonly ILogger log;

        // Key Vault
        private KeyVault keyVault;

        // Constants
        private const string CLIENT_ID = "Global:AzureActiveDirectory:aadAppId";
        private const string CLIENT_SECRET = "Global:AzureActiveDirectory:aadAppSecret";
        private const string KEY_VAULT_NAME = "Global:KeyVault:name";
        private const string ALLOWED_ACTION_KEY = "Global:Permissions";
        private const string APP_CONFIGURATION = "PCS_APPLICATION_CONFIGURATION";

        private List<string> appConfigKeys = new List<string>
        {
            "Global",
            "Global:AzureActiveDirectory",
            "Global:ClientAuth",
            "Global:ClientAuth:JWT",
            "Global:KeyVault",
            "Global:Permissions",
            "ExternalDependencies",
            "TenantManagerService",
            "StorageAdapter",
        };

        public ConfigData(ILogger logger)
        {
            this.log = logger;

            // More info about configuration at
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration
            var configurationBuilder = new ConfigurationBuilder();
#if DEBUG
            configurationBuilder.AddIniFile("appsettings.ini", optional: true, reloadOnChange: true);
#endif
            configurationBuilder.AddEnvironmentVariables();
            // build configuration with environment variables
            var preConfig = configurationBuilder.Build();
            // Add app config settings to the configuration builder
            configurationBuilder.Add(new AppConfigurationSource(preConfig[APP_CONFIGURATION], this.appConfigKeys));
            this.configuration = configurationBuilder.Build();
            // Set up Key Vault
            this.SetUpKeyVault();
        }

        public Dictionary<string, List<string>> GetUserPermissions()
        {
            Dictionary<string, List<string>> permissions = new Dictionary<string, List<string>>();
            foreach(var roleSection in this.configuration.GetSection(ALLOWED_ACTION_KEY).GetChildren())
            {
                permissions.Add(roleSection.Key,roleSection.GetChildren().Select(t => t.Key).ToList());
            }
            
            return permissions;
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
            var clientId = this.GetEnvironmentVariable(CLIENT_ID, string.Empty);
            var clientSecret = this.GetEnvironmentVariable(CLIENT_SECRET, string.Empty);
            var keyVaultName = this.GetEnvironmentVariable(KEY_VAULT_NAME, string.Empty);
            if (String.IsNullOrEmpty(clientId) || String.IsNullOrEmpty(clientSecret) || String.IsNullOrEmpty(keyVaultName))
            {
                throw new Exception("One of the required key vault keys was not configured correctly.");
            }

            // Initailize key vault
            this.keyVault = new KeyVault(keyVaultName, clientId, clientSecret, this.log);
        }

        private string GetSecrets(string key, string defaultValue = "")
        {
            string value = string.Empty;

            value = this.GetLocalVariable(key, defaultValue);

            // If secrets are not found locally, search in Key-Vault
            if (string.IsNullOrEmpty(value))
            {
                log.Warn($"Value for secret {key} not found in local env. " +
                    $" Trying to get the secret from KeyVault.", () => { });
                value = this.keyVault.GetSecret(key);
            }

            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        private string GetSecretsFromKeyVault(string key) {
            return this.keyVault.GetSecret(key);
        }

        private string GetLocalVariable(string key, string defaultValue = "")
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
                this.log.Error("Environment variables not found", () => new { varsNotFound });
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
                this.log.Warn("Environment variables not found", () => new { varsNotFound });

                notFound = true;
            }
        }
    }
}
