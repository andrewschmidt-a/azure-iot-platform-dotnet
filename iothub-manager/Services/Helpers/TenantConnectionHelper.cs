using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Config;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Helpers
{
    public class TenantConnectionHelper : ITenantConnectionHelper
    {
        private readonly IAppConfigurationClient appConfig;
        private readonly ILogger<TenantConnectionHelper> logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string TENANT_KEY = "tenant:";
        private const string IOTHUBCONNECTION_KEY = ":iotHubConnectionString";

        public TenantConnectionHelper(
            IAppConfigurationClient appConfigurationClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TenantConnectionHelper> logger)
        {
            this._httpContextAccessor = httpContextAccessor;
            this.appConfig = appConfigurationClient;
            this.logger = logger;
        }

        //Gets the tenant name from the threads current token.
        private string TenantName
        {
            get
            {
                try
                {
                    return this._httpContextAccessor.HttpContext.Request.GetTenant();
                }
                catch (Exception ex)
                {
                    throw new Exception($"A valid tenant Id was not included in the Claim. " + ex);
                }
            }
        }

        public string GetIotHubConnectionString()
        {
            var appConfigurationKey = TENANT_KEY + TenantName + IOTHUBCONNECTION_KEY;
            logger.LogDebug("App Configuration key for IoT Hub connection string for tenant {tenant} is {appConfigurationKey}", TenantName, appConfigurationKey);
            return appConfig.GetValue(appConfigurationKey);
        }

        public string GetIotHubName()
        {
            string currIoTHubHostName = null;
            IoTHubConnectionHelper.CreateUsingHubConnectionString(GetIotHubConnectionString(), (conn) =>
            {
                currIoTHubHostName = IotHubConnectionStringBuilder.Create(conn).HostName;
            });
            if (currIoTHubHostName == null)
            {
                throw new InvalidConfigurationException($"Invalid tenant information for HubConnectionString.");
            }
            return currIoTHubHostName;
        }

        public RegistryManager GetRegistry()
        {
            RegistryManager registry = null;

            IoTHubConnectionHelper.CreateUsingHubConnectionString(GetIotHubConnectionString(), (conn) =>
            {
                registry = RegistryManager.CreateFromConnectionString(conn);
            });
            if (registry == null)
            {
                throw new InvalidConfigurationException($"Invalid tenant information for HubConnectionString.");
            }
            return registry;
        }

        public JobClient GetJobClient()
        {
            JobClient job = null;

            IoTHubConnectionHelper.CreateUsingHubConnectionString(GetIotHubConnectionString(), conn =>
             {
                 job = JobClient.CreateFromConnectionString(conn);
             });
            if (job == null)
            {
                throw new InvalidConfigurationException($"Invalid tenant information for HubConnectionString.");
            }
            return job;
        }
    }
}
