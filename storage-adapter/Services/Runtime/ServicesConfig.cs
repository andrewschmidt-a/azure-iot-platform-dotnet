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
        private const string APPPLICATION_KEY = "StorageAdapter:";
        private const string TENANT_KEY = "Tenant:";
        private const string DATABASE_KEY = "Database";
        private const string COLLECTION_KEY = "Collection";
        private const string COSMOS_CONNECTION_KEY = "ConnectionString";

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
            try
            {
                return this.AppConfig[key];
            }
            catch (Exception ex)
            {
                throw new NullReferenceException($"{key} could not be found in your App Configuration instance.", ex);
            }
        }

        public string DocumentDbCollection(string tenant, string dataType)
        {
            return this.AppConfigValue($"{TENANT_KEY}{tenant}:{dataType}-{COLLECTION_KEY}");
        }
        public string DocumentDbDatabase(string dataType)
        {
            return this.AppConfigValue($"{APPPLICATION_KEY}{dataType}:{DATABASE_KEY}");

        }
        public string DocumentDbConnString
        {
            get
            {
                return this.AppConfigValue($"{APPPLICATION_KEY}{this.AdapterType}:{COSMOS_CONNECTION_KEY}");
            }
        }
    }
}
