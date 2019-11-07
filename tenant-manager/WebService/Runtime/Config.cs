// Copyright (c) Microsoft. All rights reserved.

using System;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Auth;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Runtime
{
    public interface IConfig
    {
        // Web service listening port
        int Port { get; }

        // Client authentication and authorization configuration
        IClientAuthConfig ClientAuthConfig { get; }

        // Configuration variables for services
        IServicesConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        // AppConfig
        private const string GLOBAL_KEY = "Global:";

        private const string SUBSCRIPTION_ID_KEY = GLOBAL_KEY + "subscriptionId";
        private const string LOCATION_KEY = GLOBAL_KEY + "location";
        private const string RESOURCE_GROUP_KEY = GLOBAL_KEY + "resourceGroup";

        private const string KEYVAULT_NAME_KEY = GLOBAL_KEY + "KeyVault:name";

        private const string GLOBAL_AAD_KEY = GLOBAL_KEY + "AzureActiveDirectory:";
        private const string AAD_APP_ID_KEY = GLOBAL_AAD_KEY + "aadappid";
        private const string AAD_APP_SECRET_KEY = GLOBAL_AAD_KEY + "aadappsecret";
        private const string AAD_TENANT_KEY = GLOBAL_AAD_KEY + "aadtenantid";

        private const string CLIENT_AUTH_KEY = GLOBAL_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "corsWhitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "authType";
        private const string AUTH_REQUIRED_KEY = CLIENT_AUTH_KEY + "AuthRequired";

        private const string JWT_KEY = GLOBAL_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowedAlgorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "authIssuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "audience";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clockSkewSeconds";

        private const string APPLICATION_KEY = "TenantManagerService:";
        private const string PORT_KEY = APPLICATION_KEY + "webservicePort";
        private const string AUTOMATION_ACCOUNT_KEY = APPLICATION_KEY + "automationAccountName";
        private const string TELEMETRY_CONNECTION_STRING_KEY = APPLICATION_KEY + "telemetryEventHubConnString";
        private const string LIFECYCLE_CONNECTION_STRING_KEY = APPLICATION_KEY + "lifecycleEventHubConnString";
        private const string TWIN_CHANGE_CONNECTION_STRING_KEY = APPLICATION_KEY + "twinChangeEventHubConnString";
        private const string COSMOS_DB_ENDPOINT_KEY = APPLICATION_KEY + "cosmosDbEndpoint";
        private const string COSMOS_DB_TOKEN_KEY = APPLICATION_KEY + "cosmosDbToken";
        private const string APP_CONFIG_ENDPOINT_KEY = APPLICATION_KEY + "setAppConfigEndpoint";
        private const string TENANT_MANAGER_DB_ID_KEY = APPLICATION_KEY + "databaseName";
        private const string CREATE_IOT_HUB_WEBHOOK_NAME = APPLICATION_KEY + "createIotHubWebHookName";
        private const string DELETE_IOT_HUB_WEBHOOK_NAME = APPLICATION_KEY + "deleteIotHubWebHookName";

        private const string EXTERNAL_DEPENDENCIES_KEY = "ExternalDependencies:";
        private const string IDENTITY_GATEWAY_WEBSERVICE_URL_KEY = EXTERNAL_DEPENDENCIES_KEY + "identitygatewaywebserviceurl";
        private const string CONFIG_WEBSERVICE_URL_KEY = EXTERNAL_DEPENDENCIES_KEY + "configwebserviceurl";

        private const string STORAGE_ADAPTER_DB_ID_KEY = "StorageAdapter:documentDb";

        // env/globalsecrets
        private const string APPCONFIG_CONNSTRING_KEY = "PCS_APPLICATION_CONFIGURATION";

        // KeyVault
        private const string STORAGE_ACCOUNT_CONNECTIN_STRING_KEY = "storageAccountConnectionString";
        private const string STORAGE_ACCOUNT_NAME = "Global:StorageAccount:name";
        private const string CREATE_IOT_HUB_WEBHOOK_KEY = "CreateIotHubWebHookUrl";
        private const string DELETE_IOT_HUB_WEBHOOK_KEY = "DeleteIotHubWebHookUrl";

        public int Port { get; }
        public IServicesConfig ServicesConfig { get; }
        public IClientAuthConfig ClientAuthConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);

            this.ServicesConfig = new ServicesConfig
            {
                AuthRequired = configData.GetBool(AUTH_REQUIRED_KEY),
                KeyvaultName = configData.GetString(KEYVAULT_NAME_KEY),
                AzureActiveDirectoryAppId = configData.GetString(AAD_APP_ID_KEY),
                AzureActiveDirectoryAppKey = configData.GetString(AAD_APP_SECRET_KEY),
                AzureActiveDirectoryTenant = configData.GetString(AAD_TENANT_KEY),
                SubscriptionId = configData.GetString(SUBSCRIPTION_ID_KEY),
                Location = configData.GetString(LOCATION_KEY),
                ResourceGroup = configData.GetString(RESOURCE_GROUP_KEY),
                AutomationAccountName = configData.GetString(AUTOMATION_ACCOUNT_KEY),
                TelemetryEventHubConnectionString = configData.GetString(TELEMETRY_CONNECTION_STRING_KEY),
                TwinChangeEventHubConnectionString = configData.GetString(TWIN_CHANGE_CONNECTION_STRING_KEY),
                LifecycleEventHubConnectionString = configData.GetString(LIFECYCLE_CONNECTION_STRING_KEY),
                CreateIotHubRunbookUrl = configData.GetString(CREATE_IOT_HUB_WEBHOOK_KEY),
                CreateIotHubRunbookName = configData.GetString(CREATE_IOT_HUB_WEBHOOK_NAME),
                DeleteIotHubRunbookUrl = configData.GetString(DELETE_IOT_HUB_WEBHOOK_KEY),
                DeleteIotHubRunbookName = configData.GetString(DELETE_IOT_HUB_WEBHOOK_NAME),
                AppConfigConnectionString = configData.GetString(APPCONFIG_CONNSTRING_KEY),
                AppConfigEndpoint = configData.GetString(APP_CONFIG_ENDPOINT_KEY),
                CosmosDbEndpoint = configData.GetString(COSMOS_DB_ENDPOINT_KEY),
                CosmosDbToken = configData.GetString(COSMOS_DB_TOKEN_KEY),
                TenantManagerDatabaseId = configData.GetString(TENANT_MANAGER_DB_ID_KEY),
                StorageAdapterDatabseId = configData.GetString(STORAGE_ADAPTER_DB_ID_KEY),
                UserPermissions = configData.GetUserPermissions(),
                StorageAccountConnectionString = configData.GetString(STORAGE_ACCOUNT_CONNECTIN_STRING_KEY),
                StorageAccountName = configData.GetString(STORAGE_ACCOUNT_NAME),
                IdentityGatewayWebServiceUrl = configData.GetString(IDENTITY_GATEWAY_WEBSERVICE_URL_KEY),
                ConfigWebServiceUrl = configData.GetString(CONFIG_WEBSERVICE_URL_KEY)
            };

            this.ClientAuthConfig = new ClientAuthConfig
            {
                // By default CORS is disabled
                CorsWhitelist = configData.GetString(CORS_WHITELIST_KEY, string.Empty),
                // By default Auth is required
                AuthRequired = configData.GetBool(AUTH_REQUIRED_KEY, true),
                // By default auth type is JWT
                AuthType = configData.GetString(AUTH_TYPE_KEY, "JWT"),
                // By default the only trusted algorithms are RS256, RS384, RS512
                JwtAllowedAlgos = configData.GetString(JWT_ALGOS_KEY, "RS256,RS384,RS512").Split(','),
                JwtIssuer = configData.GetString(JWT_ISSUER_KEY),
                JwtAudience = configData.GetString(JWT_AUDIENCE_KEY),
                // By default the allowed clock skew is 2 minutes
                JwtClockSkew = TimeSpan.FromSeconds(configData.GetInt(JWT_CLOCK_SKEW_KEY, 120)),
            };
        }
    }
}