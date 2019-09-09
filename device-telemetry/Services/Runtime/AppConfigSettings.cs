/*
The classes in this file define the required settings from app config
 */
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public class AppConfigSettingsProvider : ConfigurationProvider
    {
        private string appConfigConnectionString;

        // Add the parent keys for each required key (does not grab children-of-children keys)
        public static List<string> AppConfigSettingKeys = new List<string>
        {
            "Global",
            "Global:ClientAuth",
            "Global:ClientAuth:JWT",
            "Global:AzureActiveDirectory",
            "Global:Permissions",
            "TelemetryService",
            "TelemetryService:TimeSeries",
            "TelemetryService:CosmosDb",
            "TelemetryService:Messages",
            "TelemetryService:Alarms",
            "ExternalDependencies",
            "Actions"
        };

        public AppConfigSettingsProvider(string appConfigConnectionString)
        {
            this.appConfigConnectionString = appConfigConnectionString;
        }
        
        public override void Load()
        {
            // Get all app config settings in a config root
            ConfigurationBuilder appConfigBuilder = new ConfigurationBuilder();
            appConfigBuilder.AddAzureAppConfiguration(appConfigConnectionString);
            IConfigurationRoot appConfig = appConfigBuilder.Build();

            // Settings and children added to this.configuration are chosen based on elements in this list
            var appConfigKeys = AppConfigSettingsProvider.AppConfigSettingKeys;

            // Add new configurations
            foreach (var key in appConfigKeys)
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

    public class AppConfigSettingsSource : IConfigurationSource
    {
        public string appConfigConnectionString;

        public AppConfigSettingsSource(string appConfigConnectionString)
        {
            this.appConfigConnectionString = appConfigConnectionString;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AppConfigSettingsProvider(this.appConfigConnectionString);
        }
    }
}

