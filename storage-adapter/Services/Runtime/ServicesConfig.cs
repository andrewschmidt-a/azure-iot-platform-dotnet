// Copyright (c) Microsoft. All rights reserved.
using System;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageType { get; set; }
        string DocumentDbConnStringKey { get; set; }
        int DocumentDbRUs { get; set; }
        IConfigurationRoot AppConfig { get; set; }
        string DocumentDbConnString { get; }

        string DocumentDbDatabase(string dataType);
        string DocumentDbCollection(string tenant, string dataType);
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageType { get; set; }
        public string DocumentDbConnStringKey { get; set; }
        public int DocumentDbRUs { get; set; }
        public IConfigurationRoot AppConfig { get; set; }

        public ServicesConfig(
            string StorageType,
            string DocumentDbConnStringKey,
            int DocumentDbRUs,
            IAppConfigurationHelper appConfigurationHelper)
        {
            this.StorageType = StorageType;
            this.DocumentDbConnStringKey = DocumentDbConnStringKey;
            this.DocumentDbRUs = DocumentDbRUs;
            this.AppConfig = appConfigurationHelper.GetAppConfig();
        }

        private string AppConfigValue(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new NullReferenceException("App Config cannot take a null key parameter. The given key was not correctly configured.");
            }

            string value = this.AppConfig[key];
            if (String.IsNullOrEmpty(value))
            {
                throw new NullReferenceException($"App Config returned a null value for {key}");
            }

            return value;
        }

        public string DocumentDbCollection(string tenant, string dataType)
        {
            if (String.IsNullOrEmpty(tenant))
            {
                throw new NullReferenceException("The given tenant value was null. Ensure that your request has attached an ApplicationTenantId in the headers.");
            }
            return this.AppConfigValue($"tenant:{tenant}:{dataType}-collection");
        }
        public string DocumentDbDatabase(string dataType)
        {
            return this.AppConfigValue($"StorageAdapter:{dataType}");
        }

        public string DocumentDbConnString
        {
            get
            {
                return this.AppConfigValue(this.DocumentDbConnStringKey);
            }
        }
    }
}
