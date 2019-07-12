// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageAdapterApiUrl { get; }
        string UserManagementApiUrl { get; }
        string DevicePropertiesWhiteList { get; }
        string AppConfigConnection { get; }
        // ReSharper disable once InconsistentNaming
        long DevicePropertiesTTL { get; }
        long DevicePropertiesRebuildTimeout { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        //public string IoTHubConnString { get; set; }
        public string StorageAdapterApiUrl { get; set; }
        public string UserManagementApiUrl { get; set; }
        public string DevicePropertiesWhiteList { get; set; }
        public string AppConfigConnection { get; set; }
        public long DevicePropertiesTTL { get; set; }
        public long DevicePropertiesRebuildTimeout { get; set; }
    }
}
