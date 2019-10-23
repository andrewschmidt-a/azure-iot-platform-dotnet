/*
The classes in this file define the required settings from app config
 */
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace MMM.Azure.IoTSolutions.TenantManager.AppConfiguration
{
    public class AppConfigurationProvider : ConfigurationProvider
    {
        private string appConfigurationConnectionString;
        private List<string> appConfigurationKeys;

        public AppConfigurationProvider(string appConfigConnectionString, List<string> appConfigurationKeys)
        {
            this.appConfigurationConnectionString = appConfigConnectionString;
            this.appConfigurationKeys = appConfigurationKeys;
        }
        
        public override void Load()
        {
            // Get all app config settings in a config root
            ConfigurationBuilder appConfigBuilder = new ConfigurationBuilder();
            appConfigBuilder.AddAzureAppConfiguration(appConfigurationConnectionString);
            IConfigurationRoot appConfig = appConfigBuilder.Build();

            // Add new configurations
            foreach (string key in this.appConfigurationKeys)
            {
                // keys are the section strings, values are the binding instances
                IConfiguration keySettings = appConfig.GetSection(key);
                var keySettingsResult = keySettings.GetChildren().ToList();
                foreach (var config in keySettingsResult)
                {
                    // config.Path contains the full key name, rather than just the child key name
                    Data.Add(config.Path, config.Value);
                }
            }
        }
    }

    public class AppConfigurationSource : IConfigurationSource
    {
        public string appConfigConnectionString;
        public List<string> appConfigurationKeys;

        public AppConfigurationSource(string appConfigConnectionString, List<string> appConfigurationKeys)
        {
            this.appConfigConnectionString = appConfigConnectionString;
            this.appConfigurationKeys = appConfigurationKeys;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AppConfigurationProvider(this.appConfigConnectionString, this.appConfigurationKeys);
        }
    }
}
