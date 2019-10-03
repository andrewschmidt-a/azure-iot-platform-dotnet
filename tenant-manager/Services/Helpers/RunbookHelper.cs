using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using MMM.Azure.IoTSolutions.TenantManager.Services;
using MMM.Azure.IoTSolutions.TenantManager.Services.Exceptions;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using Microsoft.Azure;
using Microsoft.Azure.Management.Automation;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Helpers
{
    public class TenantRunbookHelper : IStatusOperation
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
        private const string AUTOMATION_ACCOUNT_NAME_KEY = TENANT_MANAGEMENT_KEY + "automationAccountName";

        // keyvault webhook keys
        private const string CREATE_IOT_HUB_WEBHOOK_KEY = "CreateIotHubWebHookUrl";
        private const string DELETE_IOT_HUB_WEBHOOK_KEY = "DeleteIotHubWebHookUrl";

        // injection variables
        private IConfiguration _config;
        private KeyVaultHelper _keyVaultHelper;

        private string resourceGroup;
        private string automationAccountName;

        // created in constructor
        public TokenHelper tokenHelper;
        public HttpClient httpClient;
        public AutomationManagementClient automationClient;

        // webhooks object
        // Keys refer to the actual webhook name for the particular web hook
        // Values refer to the accessor key in keyvault for that webhook's url
        public Dictionary<string, string> webHooks = new Dictionary<string, string>
        {
            { "CreateIotHub", CREATE_IOT_HUB_WEBHOOK_KEY },
            { "deleteiothub", DELETE_IOT_HUB_WEBHOOK_KEY } // note the case sensitivity here - there is a discrepency between the naming conventions of the two runbooks
        };

        public TenantRunbookHelper(IConfiguration config)
        {
            this._config = config;

            this.resourceGroup = this._config[RESOURCE_GROUP_KEY];
            this.automationAccountName = this._config[AUTOMATION_ACCOUNT_NAME_KEY];

            this.tokenHelper = new TokenHelper(this._config);
            this.httpClient = new HttpClient();
            this.automationClient = this.GetAutomationManagementClient();
        }

        public TenantRunbookHelper(IConfiguration config, KeyVaultHelper keyVaultHelper)
        {
            this._config = config;
            this._keyVaultHelper = keyVaultHelper;

            this.resourceGroup = this._config[RESOURCE_GROUP_KEY];
            this.automationAccountName = this._config[AUTOMATION_ACCOUNT_NAME_KEY];

            this.tokenHelper = new TokenHelper(this._config);
            this.httpClient = new HttpClient();
            this.automationClient = this.GetAutomationManagementClient();
        }

        /// <summary>
        /// Return the status of the create and delete runbooks
        /// </summary>
        /// <returns>StatusResultServiceModel task</returns>
        public async Task<StatusResultServiceModel> StatusAsync()
        {
            string unhealthyMessage = "";
            foreach (var webHookInfo in this.webHooks)
            {
                try
                {
                    var webHookResponse = await this.automationClient.Webhooks.GetAsync(this.resourceGroup, this.automationAccountName, webHookInfo.Key);
                    if (!webHookResponse.Webhook.Properties.IsEnabled)
                    {
                        unhealthyMessage += $"{webHookInfo.Key} is not enabled.\n";
                    }
                }
                catch (Exception e)
                {
                    unhealthyMessage += $"Unable to get status for {webHookInfo.Key}: {e.Message}";
                }
            }
            return String.IsNullOrEmpty(unhealthyMessage) ? new StatusResultServiceModel(true, "Alive and well!") : new StatusResultServiceModel(false, unhealthyMessage);
        }

        public async Task<HttpResponseMessage> CreateIotHub(string tenantId, string iotHubName)
        {
            try
            {
                return await this.TriggerTenantRunbook(this.webHooks["CreateIotHub"], tenantId, iotHubName);
            }
            catch (Exception e)
            {
                throw new RunbookTriggerException("Unable to successfully Create Iot Hub from runbook", e);
            }
        }

        public async Task<HttpResponseMessage> DeleteIotHub(string tenantId, string iotHubName)
        {
            try
            {
                return await this.TriggerTenantRunbook(this.webHooks["deleteiothub"], tenantId, iotHubName);
            }
            catch (Exception e)
            {
                throw new RunbookTriggerException("Unable to successfully Delete Iot Hub from runbook", e);
            }
        }

        /// <summary>
        /// Create the automation client using the tokenHelper and config variables to authenticate
        /// </summary>
        /// <returns>AutomationManagementClient</returns>
        private AutomationManagementClient GetAutomationManagementClient()
        {
            string authToken = this.tokenHelper.GetServicePrincipleToken();
            TokenCloudCredentials credentials = new TokenCloudCredentials(this._config[SUBSCRIPTION_ID_KEY], authToken);
            return new AutomationManagementClient(credentials);
        }

        /// <summary>
        /// Trigger a runbook for the given URL
        /// This method builds a very specific request body using configuration and the given parameters
        /// In general, the webhooks passed to this method will create or delete iot hubs
        /// </summary>
        /// <param name="webHookUrlKey" type="string">The keyvault key for the url for the runbook to trigger</param>
        /// <param name="tenantId" type="string">Tenant Guid</param>
        /// <param name="iotHubName" type="string">Iot Hub Name for deletion or creation</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> TriggerTenantRunbook(string webHookUrlKey, string tenantId, string iotHubName)
        {
            if (this._keyVaultHelper == null)
            {
                throw new RunbookTriggerException("No KeyVault Helper was configured for the RunbookHelper instance.");
            }

            var requestBody = new
            {
                tenantId = tenantId,
                iotHubName = iotHubName,
                token = this.tokenHelper.GetServicePrincipleToken(),
                resourceGroup = this.resourceGroup,
                location = this._config[LOCATION_KEY],
                subscriptionId = this._config[SUBSCRIPTION_ID_KEY],
                telemetryEventHubConnString = this._config[TELEMETRY_EVENT_HUB_CONN_STRING_KEY],
                twinChangeEventHubConnString = this._config[TWIN_CHANGE_EVENT_HUB_CONN_STRING_KEY],
                lifecycleEventHubConnString = this._config[LIFECYCLE_EVENT_HUB_CONN_STRING_KEY],
                appConfigConnectionString = this._config[APP_CONFIGURATION_KEY],
                setAppConfigEndpoint = this._config[APP_CONFIG_ENDPOINT_KEY]
            };

            try
            {
                string webHookUrl = "";
                try
                {
                    webHookUrl = await this._keyVaultHelper.GetSecretAsync(webHookUrlKey);
                    if (String.IsNullOrEmpty(webHookUrlKey))
                    {
                        throw new Exception($"KeyVault returned a null value for the web hook url at key: {webHookUrlKey}");
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to get secret from KeyVault for {webHookUrlKey}", e);
                }
                var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                return await this.httpClient.PostAsync(webHookUrl, bodyContent);
            }
            catch (Exception e)
            {
                throw new RunbookTriggerException($"Unable to successfully trigger the web hook", e);
            }
        }
    }
}