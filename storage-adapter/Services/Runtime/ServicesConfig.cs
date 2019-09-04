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
            if (String.IsNullOrEmpty(tenant))
            {
                throw new NullReferenceException("The given tenant value was null. Ensure that your request has attached an ApplicationTenantId in the headers.");
            }
            return this.AppConfig.GetValue($"tenant:{tenant}:{dataType}-collection");
        }
        
        public string DocumentDbDatabase(string dataType)
        {
            return this.AppConfig.GetValue($"StorageAdapter:{dataType}");
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
