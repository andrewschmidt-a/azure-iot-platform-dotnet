using System;
using System.Collections.Generic;
using Azure.Data.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.Helpers
{
    public class AppConfigurationHelper : IAppConfigurationHelper
    {
        private ConfigurationClient client;
        private Dictionary<string, AppConfigCacheValue> _cache = new Dictionary<string, AppConfigCacheValue>();

        public AppConfigurationHelper(IAppConfigClientConfig config)
        {
            this.client = new ConfigurationClient(config.ApplicationConfigurationConnectionString);
        }

        public AppConfigurationHelper(string applicationConfigurationConnectionString)
        {
            this.client = new ConfigurationClient(applicationConfigurationConnectionString);
        }

        /// <summary>
        /// Get a value from app config using the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The value returned by app config for the given key</returns>
        public string GetValue(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new NullReferenceException("App Config cannot take a null key parameter. The given key was not correctly configured.");
            }

            string value = "";
            try
            {
                if (this._cache.ContainsKey(key) && this._cache[key].ExpirationTime > DateTime.UtcNow)
                {
                    value = this._cache[key].Value.Value; // get string from configuration setting
                }
                else
                {
                    ConfigurationSetting setting = this.client.GetConfigurationSetting(key);
                    value = setting.Value;
                    if (this._cache.ContainsKey(key))
                    {
                        this._cache[key] = new AppConfigCacheValue(setting);
                    }
                    else
                    {
                        this._cache.Add(key, new AppConfigCacheValue(setting));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An exception occured while getting the value of {key} from App Config:\n" + e.Message);
            }

            if (String.IsNullOrEmpty(value))
            {
                throw new NullReferenceException($"App Config returned a null value for {key}");
            }
            return value;
        }
    }
}