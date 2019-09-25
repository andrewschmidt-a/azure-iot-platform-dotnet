using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Exceptions;
using ILogger = Microsoft.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
{
    public class TenantRunbookHelper
    {
        // Define constant configuration keys
        private const string APP_CONFIGURATION_KEY = "PCS_APPLICATION_CONFIGURATION";

        private const string GLOBAL_KEY = "Global:";
        private const string SUBSCRIPTION_ID_KEY = GLOBAL_KEY + "subscriptionId";
        private const string LOCATION_KEY = GLOBAL_KEY + "location";
        private const string RESOURCE_GROUP_KEY = GLOBAL_KEY + "resourceGroup";

        private const string TENANT_MANAGEMENT_KEY = "TenantManagerService:";
        private const string EVENT_HUB_CONN_STRING_SUFFIX = "EventHubConnString";
        private const string TELEMETRY_EVENT_HUB_CONN_STRING_KEY = TENANT_MANAGEMENT_KEY + "telemetry" + EVENT_HUB_CONN_STRING_SUFFIX;
        private const string LIFECYCLE_EVENT_HUB_CONN_STRING_KEY = TENANT_MANAGEMENT_KEY + "lifecycle" + EVENT_HUB_CONN_STRING_SUFFIX;
        private const string TWIN_CHANGE_EVENT_HUB_CONN_STRING_KEY = TENANT_MANAGEMENT_KEY + "twinChange" + EVENT_HUB_CONN_STRING_SUFFIX;
        private const string APP_CONFIG_ENDPOINT_KEY = TENANT_MANAGEMENT_KEY + "setAppConfigEndpoint";

        // injection variables
        private IConfiguration _config;

        // created in constructor
        public TokenHelper tokenHelper;
        public HttpClient client;

        public TenantRunbookHelper(IConfiguration config)
        {
            this._config = config;
            this.tokenHelper = new TokenHelper(this._config);
            this.client = new HttpClient();
        }

        /// <summary>
        /// Trigger a runbook for the given URL
        /// This method builds a very specific request body using configuration and the given parameters
        /// In general, the webhooks passed to this method will create or delete iot hubs
        /// </summary>
        /// <param name="webHookUrl" type="string">The url for the runbook to trigger</param>
        /// <param name="tenantId" type="string">Tenant Guid</param>
        /// <param name="iotHubName" type="string">Iot Hub Name for deletion or creation</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> TriggerTenantRunbook(string webHookUrl, string tenantId, string iotHubName)
        {
            string authToken = this.tokenHelper.GetServicePrincipleToken();

            var requestBody = new
            {
                tenantId = tenantId,
                iotHubName = iotHubName,
                token = authToken,
                location = this._config[LOCATION_KEY],
                subscriptionId = this._config[SUBSCRIPTION_ID_KEY],
                resourceGroup = this._config[RESOURCE_GROUP_KEY],
                telemetryEventHubConnString = this._config[TELEMETRY_EVENT_HUB_CONN_STRING_KEY],
                twinChangeEventHubConnString = this._config[TWIN_CHANGE_EVENT_HUB_CONN_STRING_KEY],
                lifecycleEventHubConnString = this._config[LIFECYCLE_EVENT_HUB_CONN_STRING_KEY],
                appConfigConnectionString = this._config[APP_CONFIGURATION_KEY],
                setAppConfigEndpoint = this._config[APP_CONFIG_ENDPOINT_KEY]
            };

            try
            {
                var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                return await this.client.PostAsync(webHookUrl, bodyContent);
            }
            catch (Exception e)
            {
                throw new RunbookTriggerException($"Unable to successfully trigger the runbook at {webHookUrl}", e);
            }
        }
    }
}