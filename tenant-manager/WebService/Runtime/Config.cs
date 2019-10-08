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
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string APPLICATION_KEY = "TenantManagerService:";
        private const string GLOBAL_KEY = "Global:";
        // private const string PORT_KEY = APPLICATION_KEY + "webservicePort";
        private const string CLIENT_AUTH_KEY = GLOBAL_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "corsWhitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "authType";
        private const string AUTH_REQUIRED_KEY = "AuthRequired";
        private const string JWT_KEY = GLOBAL_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowedAlgorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "authIssuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "audience";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clockSkewSeconds";

        //test
        private const string APPCONFIG_CONNSTRING_KEY = "PCS_APPLICATION_CONFIGURATION";

        public int Port { get; }
        public IClientAuthConfig ClientAuthConfig { get; }

        public Config(IConfigData configData)
        {
            // this.Port = configData.GetInt(PORT_KEY);

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