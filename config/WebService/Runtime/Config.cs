// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Microsoft.Azure.IoTSolutions.Auth;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.Runtime
{
    public interface IConfig
    {
        // Web service listening port
        int Port { get; }

        // Service layer configuration
        IServicesConfig ServicesConfig { get; }

        // Client authentication and authorization configuration
        IClientAuthConfig ClientAuthConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string APPLICATION_KEY = "ConfigService:";
        private const string GLOBAL_KEY = "Global:";
        private const string PORT_KEY = APPLICATION_KEY + "webservicePort";
        private const string SOLUTION_TYPE_KEY = APPLICATION_KEY + "solutionType";
        private const string SEED_TEMPLATE_KEY = APPLICATION_KEY + "seedTemplate";
        private const string AZURE_MAPS_KEY = APPLICATION_KEY + "azureMapsKey";

        private const string EXTERNAL_DEPENDENCIES_KEY = "ExternalDependencies:";
        private const string STORAGE_ADAPTER_URL_KEY = EXTERNAL_DEPENDENCIES_KEY + "storageAdapterWebServiceUrl";
        private const string DEVICE_SIMULATION_URL_KEY = EXTERNAL_DEPENDENCIES_KEY + "deviceSimulationWebServiceUrl";
        private const string TELEMETRY_URL_KEY = EXTERNAL_DEPENDENCIES_KEY + "telemetryWebServiceUrl";

        private const string DEVICE_SIMULATION_KEY = "DeviceSimulationService:";
        private const string TELEMETRY_KEY = "TelemetryService:";

        private const string CLIENT_AUTH_KEY = GLOBAL_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "corsWhitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "authType";
        private const string AUTH_REQUIRED_KEY = "AuthRequired";

        private const string JWT_KEY = GLOBAL_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowedAlgorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "authissuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "audience";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clockSkewSeconds";

        private const string USER_MANAGEMENT_KEY = "UserManagementService:";
        private const string USER_MANAGEMENT_URL_KEY = USER_MANAGEMENT_KEY + "authWebServiceUrl";

        private const string ACTIONS_KEY = APPLICATION_KEY + "Actions:";
        private const string OFFICE365_LOGIC_APP_URL_KEY = ACTIONS_KEY + "office365ConnectionUrl";
        private const string RESOURCE_GROUP_KEY = ACTIONS_KEY + "solutionName";
        private const string SUBSCRIPTION_ID_KEY = ACTIONS_KEY + "subscriptionId";
        private const string MANAGEMENT_API_VERSION_KEY = ACTIONS_KEY + "managementApiVersion";
        private const string ARM_ENDPOINT_URL_KEY = ACTIONS_KEY + "armEndpointUrl";

        public int Port { get; }
        public IServicesConfig ServicesConfig { get; }
        public IClientAuthConfig ClientAuthConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);

            this.ServicesConfig = new ServicesConfig
            {
                StorageAdapterApiUrl = configData.GetString(STORAGE_ADAPTER_URL_KEY),
                DeviceSimulationApiUrl = configData.GetString(DEVICE_SIMULATION_URL_KEY),
                TelemetryApiUrl = configData.GetString(TELEMETRY_URL_KEY),
                SolutionType = configData.GetString(SOLUTION_TYPE_KEY),
                SeedTemplate = configData.GetString(SEED_TEMPLATE_KEY),
                AzureMapsKey = configData.GetString(AZURE_MAPS_KEY),
                UserManagementApiUrl = configData.GetString(USER_MANAGEMENT_URL_KEY),
                Office365LogicAppUrl = configData.GetString(OFFICE365_LOGIC_APP_URL_KEY),
                ResourceGroup = configData.GetString(RESOURCE_GROUP_KEY),
                SubscriptionId = configData.GetString(SUBSCRIPTION_ID_KEY),
                ManagementApiVersion = configData.GetString(MANAGEMENT_API_VERSION_KEY),
                ArmEndpointUrl = configData.GetString(ARM_ENDPOINT_URL_KEY),
                UserPermissions = configData.GetUserPermissions()
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
                JwtSecurityKeys = null,
                // By default the allowed clock skew is 2 minutes
                JwtClockSkew = TimeSpan.FromSeconds(configData.GetInt(JWT_CLOCK_SKEW_KEY, 120)),
            };
        }
    }
}
