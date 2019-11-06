using System.Collections.Generic;
using MMM.Azure.IoTSolutions.TenantManager.Services.Helpers; 

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Runtime
{
    public interface IServicesConfig
    {
        bool AuthRequired { get; set; }

        string KeyvaultName { get; set; }
        string AzureActiveDirectoryAppId { get; set; }
        string AzureActiveDirectoryAppKey { get; set; }
        string AzureActiveDirectoryTenant { get; set; }
        string ResourceGroup { get; set; }
        string SubscriptionId { get; set; }
        string Location { get; set; }
        string AutomationAccountName { get; set; }
        string TelemetryEventHubConnectionString { get; set; }
        string TwinChangeEventHubConnectionString { get; set; }
        string LifecycleEventHubConnectionString { get; set; }
        string AppConfigConnectionString { get; set; }
        string AppConfigEndpoint { get; set; }
        string CosmosDbEndpoint { get; set; }
        string CosmosDbToken { get; set; }
        string TenantManagerDatabaseId { get; set; }
        string StorageAdapterDatabseId { get; set; }
        string StorageAccountConnectionString { get; set; }
        string StorageAccountName { get; set; }
        string CreateIotHubRunbookUrl { get; set; }
        string CreateIotHubRunbookName { get; set; }
        string DeleteIotHubRunbookUrl { get; set; }
        string DeleteIotHubRunbookName { get; set;}
        string IdentityGatewayWebServiceUrl { get; set; }
        string ConfigWebServiceUrl { get; set; }

        Dictionary<string, List<string>> UserPermissions { get; set; }
    }

    public class ServicesConfig: IServicesConfig
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
        public string TelemetryEventHubConnectionString { get; set; }
        public string TwinChangeEventHubConnectionString { get; set; }
        public string LifecycleEventHubConnectionString { get; set; }
        public string AppConfigConnectionString { get; set; }
        public string AppConfigEndpoint { get; set; }
        public string CosmosDbEndpoint { get; set; }
        public string CosmosDbToken { get; set; }
        public string TenantManagerDatabaseId { get; set; }
        public string StorageAdapterDatabseId { get; set; }
        public string StorageAccountConnectionString { get; set; }
        public string StorageAccountName {get; set;}
        public string CreateIotHubRunbookUrl { get; set; }
        public string CreateIotHubRunbookName { get; set; }
        public string DeleteIotHubRunbookUrl { get; set; }
        public string DeleteIotHubRunbookName { get; set;}
        public string IdentityGatewayWebServiceUrl { get; set; }
        public string ConfigWebServiceUrl { get; set; }
    
        public Dictionary<string, List<string>> UserPermissions { get; set; }
    }
}