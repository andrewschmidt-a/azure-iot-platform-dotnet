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
        IAppConfigurationHelper AppConfig { get; set; }
        string DocumentDbConnString { get; }

        string DocumentDbDatabase(string dataType);
        string DocumentDbCollection(string tenant, string dataType);
    }

    public class ServicesConfig : IServicesConfig
    {
        private const string APPPLICATION_KEY = "StorageAdapter:";
        private const string TENANT_KEY = "Tenant:";
        private const string COLLECTION_KEY = "Collection";

        public string StorageType { get; set; }
        public string DocumentDbConnStringKey { get; set; }
        public int DocumentDbRUs { get; set; }
        public IAppConfigurationHelper AppConfig { get; set; }

        public ServicesConfig(
            string StorageType,
            string DocumentDbConnStringKey,
            int DocumentDbRUs,
            IAppConfigurationHelper appConfigurationHelper)
        {
            this.StorageType = StorageType;
            this.DocumentDbConnStringKey = DocumentDbConnStringKey;
            this.DocumentDbRUs = DocumentDbRUs;
            this.AppConfig = appConfigurationHelper;
        }

        public string DocumentDbCollection(string tenant, string dataType)
        {
            return this.AppConfig.GetValue($"{TENANT_KEY}{tenant}:{dataType}-{COLLECTION_KEY}");
        }

        public string DocumentDbDatabase(string dataType)
        {
           return this.AppConfig.GetValue($"{APPPLICATION_KEY}{dataType}");
        }
        

        public string DocumentDbConnString
        {
            get
            {
                return this.AppConfig.GetValue(this.DocumentDbConnStringKey);
            }
        }
    }
}
