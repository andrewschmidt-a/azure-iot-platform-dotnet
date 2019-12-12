// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services.Auth;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.Config.Services.Runtime
{
    public interface IServicesConfig : IStorageAdapterClientConfig, IAuthMiddlewareConfig, IUserManagementClientConfig, IAsaManagerClientConfig
    {
        string SolutionType { get; set; }
        string DeviceSimulationApiUrl { get; }
        string TelemetryApiUrl { get; }
        string SeedTemplate { get; }
        string AzureMapsKey { get; }
        string Office365LogicAppUrl { get; }
        string ResourceGroup { get; }
        string SubscriptionId { get; }
        string ManagementApiVersion { get; }
        string ArmEndpointUrl { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string AsaManagerApiUrl { get; set; }
        public int StorageAdapterApiTimeout { get; set; }
        public string SolutionType { get; set; }
        public string StorageAdapterApiUrl { get; set; }
        public string DeviceSimulationApiUrl { get; set; }
        public string TelemetryApiUrl { get; set; }
        public string SeedTemplate { get; set; }
        public string AzureMapsKey { get; set; }
        public string UserManagementApiUrl { get; set; }
        public string Office365LogicAppUrl { get; set; }
        public string ResourceGroup { get; set; }
        public string SubscriptionId { get; set; }
        public string ManagementApiVersion { get; set; }
        public string ArmEndpointUrl { get; set; }
        public Dictionary<string, List<string>> UserPermissions { get; set; }
    }
}
