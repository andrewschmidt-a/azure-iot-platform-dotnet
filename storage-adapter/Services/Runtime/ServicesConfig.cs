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
        private const string APPPLICATION_KEY = "StorageAdapter:";
        private const string TENANT_KEY = "Tenant:";
        private const string DATABASE_KEY = "Database";
        private const string COLLECTION_KEY = "Collection";
        private const string COSMOS_CONNECTION_KEY = "ConnectionString";

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
            return this.AppConfigValue($"{TENANT_KEY}{tenant}:{dataType}-{COLLECTION_KEY}");
        }

        public string DocumentDbDatabase(string dataType)
        {
           return this.AppConfigValue($"{APPPLICATION_KEY}{dataType}");
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
