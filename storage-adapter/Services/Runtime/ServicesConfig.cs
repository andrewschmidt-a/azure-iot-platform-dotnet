// Copyright (c) Microsoft. All rights reserved.
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageType { get; set; }
        string AdapterType { get; set; }
        string DocumentDbConnString { get; }
        string AppConfigConnString { get; set; }
        int DocumentDbRUs { get; set; }

        string DocumentDbDatabase(string dataType);
        string DocumentDbCollection(string tenant, string dataType);
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageType { get; set; }
        public string AdapterType { get; set; }
        public int DocumentDbRUs { get; set; }
        public string AppConfigConnString { get; set; }
        public IConfigurationRoot AppConfig;

        public ServicesConfig(
            string StorageType,
            string AdapterType,
            int DocumentDbRUs,
            string AppConfigConnString)
        {
            this.StorageType = StorageType;
            this.AdapterType = AdapterType;
            this.DocumentDbRUs = DocumentDbRUs;
            this.AppConfigConnString = AppConfigConnString;
            this.AppConfig = AppConfigurationHelper.GetAppConfig(this.AppConfigConnString);
        }

        public string DocumentDbCollection(string tenant, string dataType)
        {
            return this.AppConfig[$"tenant:{dataType}-collection:{tenant}"];
        }

        public string DocumentDbDatabase(string dataType)
        {
            return this.AppConfig["storage-adapter:database:" + dataType];
        }

        public string DocumentDbConnString
        {
            get
            {
                return this.AppConfig["storage-adapter:connstring:" + this.AdapterType];
            }
        }
    }
}
