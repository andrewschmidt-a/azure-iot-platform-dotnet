// Copyright (c) Microsoft. All rights reserved.

using System;
using Mmm.Platform.IoT.StorageAdapter.Services.Runtime;
using Mmm.Platform.IoT.StorageAdapter.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Runtime;

namespace Mmm.Platform.IoT.StorageAdapter.WebService.Runtime
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
        private const string GLOBAL_KEY = "Global:";
        private const string APP_INSIGHTS_INSTRUMENTATION_KEY = GLOBAL_KEY + "instrumentationKey";

        private const string APPLICATION_KEY = "StorageAdapter:";
        private const string PORT_KEY = APPLICATION_KEY + "webservicePort";
        private const string STORAGE_TYPE_KEY = APPLICATION_KEY + "storageType";
        private const string DOCUMENT_DB_RUS_KEY = APPLICATION_KEY + "documentDBRUs";

        private const string EXTERNAL_DEPENDENCIES = "ExternalDependencies:";
        private const string USER_MANAGEMENT_URL_KEY = EXTERNAL_DEPENDENCIES + "authWebServiceUrl";

        private const string COSMOS_CONNECTION_STRING_KEY = "documentDBConnectionString";

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        /// <summary>Service layer configuration</summary>
        public IServicesConfig ServicesConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);
            this.ServicesConfig = new ServicesConfig
            {
                StorageType = configData.GetString(STORAGE_TYPE_KEY),
                DocumentDbConnString = configData.GetString(COSMOS_CONNECTION_STRING_KEY),
                DocumentDbRUs = configData.GetInt(DOCUMENT_DB_RUS_KEY),
                UserManagementApiUrl = configData.GetString(USER_MANAGEMENT_URL_KEY),
                ApplicationConfigurationConnectionString = configData.AppConfigurationConnectionString
            };

            AppInsightsExceptionHelper.Initialize(configData.GetString(APP_INSIGHTS_INSTRUMENTATION_KEY));
        }
    }
}
