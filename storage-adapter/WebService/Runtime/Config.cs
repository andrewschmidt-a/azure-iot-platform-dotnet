// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.Runtime
{
    public interface IConfig
    {
        /// <summary>Web service listening port</summary>
        int Port { get; }

        /// <summary>Service layer configuration</summary>
        IServicesConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string COSMOS_CONNECTION_STRING_APP_CONFIG_KEY = "cosmos-odin-mt-proc";
        private const string APPLICATION_KEY = "StorageAdapter:";
        private const string PORT_KEY = APPLICATION_KEY + "webservicePort";
        private const string STORAGE_TYPE_KEY = APPLICATION_KEY + "storageType";
        private const string DOCUMENT_DB_RUS_KEY = APPLICATION_KEY + "documentDBRUs";
        private const string APP_CONFIG_CONNECTION_STRING_KEY = APPLICATION_KEY + "appConfigConnectionString";

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        /// <summary>Service layer configuration</summary>
        public IServicesConfig ServicesConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);

            var storageType = configData.GetString(STORAGE_TYPE_KEY).ToLowerInvariant();
            var appConfigConnectionString = configData.GetString(APP_CONFIG_CONNECTION_STRING_KEY); //.ToLowerInvariant();
            if (storageType == "documentdb" &&
                (string.IsNullOrEmpty(appConfigConnectionString)
                 || appConfigConnectionString.StartsWith("${")
                 || appConfigConnectionString.Contains("...")))
            {
                // In order to connect to the storage, the service requires a connection
                // string for Document Db. The value can be found in the Azure Portal.
                // The connection string can be stored in the 'appsettings.ini' configuration
                // file, or in the PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING environment variable.
                // When working with VisualStudio, the environment variable can be set in the
                // WebService project settings, under the "Debug" tab.
                throw new Exception("The service configuration is incomplete. " +
                                    "Please provide your App Config connection string. " +
                                    "For more information, see the environment variables " +
                                    "used in project properties and the 'appConfigurationConnectionString' " +
                                    "value in the 'appsettings.ini' configuration file.");
            }
            AppConfigurationHelper appConfig = new AppConfigurationHelper(appConfigConnectionString);
            this.ServicesConfig = new ServicesConfig(storageType, COSMOS_CONNECTION_STRING_APP_CONFIG_KEY, configData.GetInt(DOCUMENT_DB_RUS_KEY), appConfig);
        }
    }
}
