using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Mmm.Platform.IoT.Common.AuthUtils;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Helpers;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers
{
    public interface ITenantConnectionHelper
    {
        string getIoTHubName();
        RegistryManager getRegistry();
        string getIoTHubConnectionString();
        JobClient GetJobClient();
    }
    public class TenantConnectionHelper : ITenantConnectionHelper
    {
        //Gets the tenant name from the threads current token.
        private string tenantName
        {
            get
            {
                // return $"{((Dictionary<string, string>)((ClaimsPrincipal)Thread.CurrentPrincipal).Claims)["tenant"]}-tenant-data";
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

        private IAppConfigurationHelper appConfig;
        private IHttpContextAccessor _httpContextAccessor;

        private const string TENANT_KEY = "tenant:";
        private const string IOTHUBCONNECTION_KEY = ":iotHubConnectionString";

        /// <summary>
        /// Create connection to appconfig resource
        /// </summary>
        /// <param name="appConfigConnection">Connection string for app config</param>
        /// <returns></returns>
        public TenantConnectionHelper(IHttpContextAccessor httpContextAccessor, IAppConfigClientConfig config)
        {
            this._httpContextAccessor = httpContextAccessor;
            this.appConfig = new AppConfigurationHelper(config);
        }

        public TenantConnectionHelper()
        {
        }

        /// <summary>
        /// Returns the iothubconnection string for a given tenant
        /// </summary>
        /// <returns>iothub connection string</returns>
        public string getIoTHubConnectionString()
        {
            return appConfig.GetValue(TENANT_KEY + tenantName + IOTHUBCONNECTION_KEY);
        }

        public string getIoTHubName()
        {
            string currIoTHubHostName = null;
            IoTHubConnectionHelper.CreateUsingHubConnectionString(getIoTHubConnectionString(), (conn) =>
            {
                currIoTHubHostName = IotHubConnectionStringBuilder.Create(conn).HostName;
            });
            if (currIoTHubHostName == null)
            {
                throw new InvalidConfigurationException($"Invalid tenant information for HubConnectionString.");
            }
            return currIoTHubHostName;
        }

        /// <summary>
        /// Return current registry based on tenant from token
        /// </summary>
        /// <returns>Registry</returns>
        public RegistryManager getRegistry()
        {
            RegistryManager registry = null;

            IoTHubConnectionHelper.CreateUsingHubConnectionString(getIoTHubConnectionString(), (conn) =>
            {
                registry = RegistryManager.CreateFromConnectionString(conn);
            });
            if (registry == null)
            {
                throw new InvalidConfigurationException($"Invalid tenant information for HubConnectionString.");
            }
            return registry;
        }
        /// <summary>
        /// Return current job based on tenant from token
        /// </summary>
        /// <returns>job</returns>
        public JobClient GetJobClient()
        {
            JobClient job = null;

            IoTHubConnectionHelper.CreateUsingHubConnectionString(getIoTHubConnectionString(), conn =>
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
