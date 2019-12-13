using System;
using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services.Auth;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;

namespace Mmm.Platform.IoT.TenantManager.Services.Runtime
{
    public interface IServicesConfig : IAppConfigClientConfig, IUserManagementClientConfig, IAuthMiddlewareConfig, IStorageClientConfig, ITableStorageClientConfig
    {
        string KeyvaultName { get; set; }
        string AzureActiveDirectoryAppId { get; set; }
        string AzureActiveDirectoryAppKey { get; set; }
        string AzureActiveDirectoryTenant { get; set; }
        string ResourceGroup { get; set; }
        string SubscriptionId { get; set; }
        string Location { get; set; }
        string AutomationAccountName { get; set; }
        string EventHubNamespaceName { get; set; }
        string EventHubAccessPolicyKey { get; set; }
        string TelemetryEventHubConnectionString { get; set; }
        string TwinChangeEventHubConnectionString { get; set; }
        string LifecycleEventHubConnectionString { get; set; }
        string AppConfigEndpoint { get; set; }
        string CosmosDbAccount { get; set; }
        string CosmosDbKey { get; set; }
        string StreamAnalyticsDatabaseId { get; set; }
        string TenantManagerDatabaseId { get; set; }
        string StorageAdapterDatabseId { get; set; }
        string StorageAccountName { get; set; }
        string StorageAccountKey { get; set; }
        string CreateIotHubRunbookUrl { get; set; }
        string DeleteIotHubRunbookUrl { get; set; }
        string CreateStreamAnalyticsRunbookUrl { get; set; }
        string DeleteStreamAnalyticsRunbookUrl { get; set; }
        string IdentityGatewayWebServiceUrl { get; set; }
        string ConfigWebServiceUrl { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public bool AuthRequired { get; set; }

        public string KeyvaultName { get; set; }
        public string AzureActiveDirectoryAppId { get; set; }
        public string AzureActiveDirectoryAppKey { get; set; }
        public string AzureActiveDirectoryTenant { get; set; }
        public string ResourceGroup { get; set; }
        public string SubscriptionId { get; set; }
        public string Location { get; set; }
        public string AutomationAccountName { get; set; }
        public string EventHubNamespaceName { get; set; }
        public string EventHubAccessPolicyKey { get; set; }
        public string TelemetryEventHubConnectionString { get; set; }
        public string TwinChangeEventHubConnectionString { get; set; }
        public string LifecycleEventHubConnectionString { get; set; }
        public string AppConfigEndpoint { get; set; }
        public string CosmosDbAccount { get; set; }
        public string CosmosDbKey { get; set; }
        public string CosmosDbConnectionString { get; set; }
        public int CosmosDbThroughput { get; set; }
        public string StreamAnalyticsDatabaseId { get; set; }
        public string TenantManagerDatabaseId { get; set; }
        public string StorageAdapterDatabseId { get; set; }
        public string StorageAccountConnectionString { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string CreateIotHubRunbookUrl { get; set; }
        public string DeleteIotHubRunbookUrl { get; set; }
        public string CreateStreamAnalyticsRunbookUrl { get; set; }
        public string DeleteStreamAnalyticsRunbookUrl { get; set; }
        public string IdentityGatewayWebServiceUrl { get; set; }
        public string ConfigWebServiceUrl { get; set; }
        public Dictionary<string, List<string>> UserPermissions { get; set; }
        public string ApplicationConfigurationConnectionString { get; set; }
        public string UserManagementApiUrl { get; set; }
    }
}