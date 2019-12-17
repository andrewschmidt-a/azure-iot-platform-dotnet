// Copyright (c) Microsoft. All rights reserved.

using System;
using Mmm.Platform.IoT.AsaManager.Services.Runtime;
using Mmm.Platform.IoT.Common.Services.Auth;
using Mmm.Platform.IoT.Common.Services.Runtime;

namespace Mmm.Platform.IoT.AsaManager.WebService.Runtime
{
    public interface IConfig
    {
        // Web service listening port
        int Port { get; }

        // Configuration variables for services
        IServicesConfig ServicesConfig { get; }
        IClientAuthConfig ClientAuthConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string GLOBAL_KEY = "Global:";

        private const string CLIENT_AUTH_KEY = GLOBAL_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "corsWhitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "authType";
        private const string AUTH_REQUIRED_KEY = CLIENT_AUTH_KEY + "authrequired";

        private const string JWT_KEY = GLOBAL_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowedAlgorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "authissuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "aadAppId";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clockSkewSeconds";

        private const string PORT_KEY = "AsaManagerService:webserviceport";

        private const string EXTERNAL_DEPENDENCY_KEY = "ExternalDependencies:";
        private const string IOTHUB_MANAGER_SERVICE_URL = EXTERNAL_DEPENDENCY_KEY + "iothubmanagerwebserviceurl";
        private const string STORAGE_ADAPTER_SERVICE_URL = EXTERNAL_DEPENDENCY_KEY + "storageadapterwebserviceurl";
        private const string STORAGE_ADAPTER_TIMEOUT = EXTERNAL_DEPENDENCY_KEY + "storageadapterwebservicetimeout";

        // keyvault
        private const string STORAGE_ACCOUNT_CONNECTIN_STRING_KEY = "storageAccountConnectionString";

        public int Port { get; }
        public IServicesConfig ServicesConfig { get; }
        public IClientAuthConfig ClientAuthConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);

            this.ServicesConfig = new ServicesConfig
            {
                IotHubManagerApiUrl = configData.GetString(IOTHUB_MANAGER_SERVICE_URL),
                StorageAccountConnectionString = configData.GetString(STORAGE_ACCOUNT_CONNECTIN_STRING_KEY),
                StorageAdapterApiUrl = configData.GetString(STORAGE_ADAPTER_SERVICE_URL),
                StorageAdapterApiTimeout = configData.GetInt(STORAGE_ADAPTER_TIMEOUT)
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