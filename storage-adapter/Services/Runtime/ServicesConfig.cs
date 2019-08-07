// Copyright (c) Microsoft. All rights reserved.
using System;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageType { get; set; }
        string AdapterType { get; set; }
        string DocumentDbConnString { get; }
        int DocumentDbRUs { get; set; }
        IConfigurationRoot AppConfig { get; set; }

        string DocumentDbDatabase(string dataType);
        string DocumentDbCollection(string tenant, string dataType);
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageType { get; set; }
        public string AdapterType { get; set; }
        public int DocumentDbRUs { get; set; }
        public IConfigurationRoot AppConfig { get; set; }

        public ServicesConfig(
            string StorageType,
            string AdapterType,
            int DocumentDbRUs,
            IAppConfigurationHelper appConfigurationHelper)
        {
            this.StorageType = StorageType;
            this.AdapterType = AdapterType;
            this.DocumentDbRUs = DocumentDbRUs;
            this.AppConfig = appConfigurationHelper.GetAppConfig();
        }

        private string AppConfigValue(string key)
        {
            string value = this.AppConfig[key];
            if (String.IsNullOrEmpty(value))
            {
                throw new NullReferenceException($"App Config returned a null value for {key}");
            }
            return value;
        }

        public string DocumentDbCollection(string tenant, string dataType)
        {
            return this.AppConfigValue($"tenant:{tenant}:{dataType}-collection");
        }
        public string DocumentDbDatabase(string dataType)
        {
            return this.AppConfigValue($"storage-adapter:{dataType}:database");
        }
        public string DocumentDbConnString
        {
            get
            {
                return this.AppConfigValue($"storage-adapter:{this.AdapterType}:connstring");
            }
        }
    }
}
