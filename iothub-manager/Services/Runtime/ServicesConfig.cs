// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services.Auth;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Helpers;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Runtime
{
    public interface IServicesConfig : IStorageAdapterClientConfig, IUserManagementClientConfig, IAppConfigClientConfig, IAuthMiddlewareConfig
    {
        string DevicePropertiesWhiteList { get; }
        long DevicePropertiesTTL { get; }
        long DevicePropertiesRebuildTimeout { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public int StorageAdapterApiTimeout { get; set; }
        public string StorageAdapterApiUrl { get; set; }
        public string UserManagementApiUrl { get; set; }
        public string DevicePropertiesWhiteList { get; set; }
        public long DevicePropertiesTTL { get; set; }
        public long DevicePropertiesRebuildTimeout { get; set; }
        public Dictionary<string, List<string>> UserPermissions { get; set; }
        public string ApplicationConfigurationConnectionString { get; set; }
    }
}
