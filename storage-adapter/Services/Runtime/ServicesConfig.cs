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
        string DocumentDbConnString { get; set; }
        int DocumentDbRUs { get; set; }
        string UserManagementApiUrl { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public bool AuthRequired { get; set; }
        public string StorageType { get; set; }
        public string DocumentDbConnString { get; set; }
        public int DocumentDbRUs { get; set; }
        public string UserManagementApiUrl { get; set; }
        public string ApplicationConfigurationConnectionString { get; set; }
    }
}
