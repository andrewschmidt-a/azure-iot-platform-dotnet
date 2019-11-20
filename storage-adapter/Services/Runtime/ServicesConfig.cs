// Copyright (c) Microsoft. All rights reserved.
using System;
using Mmm.Platform.IoT.StorageAdapter.Services.Helpers;
using Microsoft.Extensions.Configuration;
using Mmm.Platform.IoT.Common.Services.Helpers;

namespace Mmm.Platform.IoT.StorageAdapter.Services.Runtime
{
    public interface IServicesConfig : IAppConfigClientConfig
    {
        bool AuthRequired { get; set; }
        string StorageType { get; set; }
        string DocumentDbDatabase { get; set; }
        string DocumentDbConnString { get; set; }
        int DocumentDbRUs { get; set; }
        string UserManagementApiUrl { get; set; }
        IAppConfigurationHelper AppConfig { get; set; }

        string DocumentDbCollection(string tenant, string dataType);
    }

    public class ServicesConfig : IServicesConfig
    {
        private const string APPPLICATION_KEY = "StorageAdapter:";
        private const string TENANT_KEY = "tenant:";
        private const string COLLECTION_KEY = "collection";

        public bool AuthRequired { get; set; }
        public string StorageType { get; set; }
        public string DocumentDbDatabase { get; set; }
        public string DocumentDbConnString { get; set; }
        public int DocumentDbRUs { get; set; }
        public string UserManagementApiUrl { get; set; }
        public IAppConfigurationHelper AppConfig { get; set; }
        public string ApplicationConfigurationConnectionString { get; }

        /// <summary>
        /// Use the object initializer pattern to set the object's attributes
        /// bool AuthRequire
        /// string StorageType
        /// string DocumentDbConnStringKey
        /// int DocumentDbRUs
        /// string UserManagementApiUrl
        /// IConfigurationRoot AppConfig
        /// </summary>
        public ServicesConfig() { }

        public string DocumentDbCollection(string tenant, string dataType)
        {
            return this.AppConfig.GetValue($"{TENANT_KEY}{tenant}:{dataType}-{COLLECTION_KEY}");
        }
    }
}
