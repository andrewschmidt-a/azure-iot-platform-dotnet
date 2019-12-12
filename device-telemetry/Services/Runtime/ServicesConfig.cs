// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mmm.Platform.IoT.Common.Services.Auth;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.Services.Helpers;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Runtime
{
    public interface IServicesConfig : IAppConfigClientConfig, IStorageClientConfig, ITimeSeriesClientConfig, IUserManagementClientConfig, IStorageAdapterClientConfig, IAuthMiddlewareConfig, IAsaManagerClientConfig
    {
        StorageConfig MessagesConfig { get; set; }
        AlarmsConfig AlarmsConfig { get; set; }
        string StorageType { get; set; }
        string DiagnosticsApiUrl { get; }
        int DiagnosticsMaxLogRetries { get; }
        string ActionsEventHubConnectionString { get; }
        string ActionsEventHubName { get; }
        string BlobStorageConnectionString { get; }
        string ActionsBlobStorageContainer { get; }
        string LogicAppEndpointUrl { get; }
        string SolutionUrl { get; }
        string TemplateFolder { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string AsaManagerApiUrl { get; set; }

        public string StorageAdapterApiUrl { get; set; }

        public int StorageAdapterApiTimeout { get; set; }

        public string UserManagementApiUrl { get; set; }

        public StorageConfig MessagesConfig { get; set; }

        public AlarmsConfig AlarmsConfig { get; set; }

        public string StorageType { get; set; }

        public int CosmosDbThroughput { get; set; }

        public string DiagnosticsApiUrl { get; set; }

        public int DiagnosticsMaxLogRetries { get; set; }

        public string CosmosDbConnectionString { get; set; }

        public string TimeSeriesFqdn { get; set; }

        public string TimeSeriesAuthority { get; set; }

        public string TimeSeriesAudience { get; set; }

        public string TimeSeriesExplorerUrl { get; set; }

        public string TimeSertiesApiVersion { get; set; }

        public string TimeSeriesTimeout { get; set; }

        public string ActiveDirectoryTenant { get; set; }

        public string ActiveDirectoryAppId { get; set; }

        public string ActiveDirectoryAppSecret { get; set; }

        public string ActionsEventHubConnectionString { get; set; }

        public string ActionsEventHubName { get; set; }

        public string BlobStorageConnectionString { get; set; }

        public string ActionsBlobStorageContainer { get; set; }

        public string LogicAppEndpointUrl { get; set; }

        public string SolutionUrl { get; set; }

        public string TemplateFolder { get; set; }

        public Dictionary<string, List<string>> UserPermissions { get; set; }

        public string ApplicationConfigurationConnectionString { get; set; }
    }
}