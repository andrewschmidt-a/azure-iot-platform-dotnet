using System;
using Azure.ApplicationModel.Configuration;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers
{
    public interface IAppConfigurationHelper
    {
        string GetValue(string key);
    }

    public class AppConfigurationHelper : IAppConfigurationHelper
    {
        private ConfigurationClient client;
        private IServicesConfig _config;

        public AppConfigurationHelper(IServicesConfig config)
        {
            this.client = new ConfigurationClient(config.ApplicationConfigurationConnectionString);
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
                ConfigurationSetting setting = this.client.Get(key);
                value = setting.Value;
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