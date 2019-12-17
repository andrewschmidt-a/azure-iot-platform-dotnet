// <copyright file="AppConfigurationProvider.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Mmm.Platform.IoT.Common.Services
{
    public class AppConfigurationProvider : ConfigurationProvider
    {
        private readonly string appConfigurationConnectionString;
        private readonly List<string> appConfigurationKeys;

        public AppConfigurationProvider(string appConfigConnectionString, List<string> appConfigurationKeys)
        {
            this.appConfigurationConnectionString = appConfigConnectionString;
            this.appConfigurationKeys = appConfigurationKeys;
        }

        public override void Load()
        {
            // Get all app config settings in a config root
            ConfigurationBuilder appConfigBuilder = new ConfigurationBuilder();
            appConfigBuilder.AddAzureAppConfiguration(this.appConfigurationConnectionString);
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
                    this.Data.Add(config.Path, config.Value);
                }
            }
        }
    }
}
