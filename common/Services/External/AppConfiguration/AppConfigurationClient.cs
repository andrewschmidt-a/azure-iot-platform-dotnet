using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Azure.Data.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.External.AppConfiguration
{
    public class AppConfigurationClient : IAppConfigurationClient
    {
        private ConfigurationClient client;
        private string statusKey;
        private Dictionary<string, AppConfigCacheValue> _cache = new Dictionary<string, AppConfigCacheValue>();

        public AppConfigurationClient(AppConfig config, string statusKey = "Global")
        {
            this.client = new ConfigurationClient(config.AppConfigurationConnectionString);
            this.statusKey = statusKey;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                await this.client.GetConfigurationSettingAsync(this.statusKey);
                return new StatusResultServiceModel(true, "Alive and well!");
            }
            catch (Exception)
            {
                return new StatusResultServiceModel(false, $"Unable to retrieve basic key \"{this.statusKey}\" from app config.");
            }
        }

        /// <summary>
        /// Create a new App Config key value pair
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>void</returns>
        public async Task SetValueAsync(string key, string value)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("the key parameter must not be null or empty to create a new app config key value pair.");
            }

            try
            {
                await this.client.SetConfigurationSettingAsync(key, value);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to create the app config key value pair {{{key}, {value}}}", e);
            }
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
                throw new ArgumentNullException("App Config cannot take a null key parameter. The given key was not correctly configured.");
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

        /// <summary>
        /// Delete an app config key value pair based on key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task DeleteKeyAsync(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("The key parameter must not be null or empty to delete an app config key value pair.");
            }

            try
            {
                await this.client.DeleteConfigurationSettingAsync(key);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to delete the app config key value pair for key {key}", e);
            }
        }
    }
}