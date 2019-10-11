using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;
using MMM.Azure.IoTSolutions.TenantManager.Services.Exceptions;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using Microsoft.Azure;
using Microsoft.Azure.Management.Automation;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Helpers
{
    public class TenantRunbookHelper : IStatusOperation
    {
        // injection variables
        private IServicesConfig _config;

        private string resourceGroup;
        private string automationAccountName;

        // created in constructor
        public TokenHelper _tokenHelper;
        public HttpClient httpClient;
        public AutomationManagementClient automationClient;

        // webhooks object
        // Keys refer to the actual webhook name for the particular web hook
        // Values refer to the accessor key in config for that webhook's url
        public Dictionary<string, string> webHooks; 

        public TenantRunbookHelper(IServicesConfig config, TokenHelper tokenHelper)
        {
            this._tokenHelper = tokenHelper;
            this._config = config;
            
            this.resourceGroup = this._config.ResourceGroup;
            this.automationAccountName = this._config.AutomationAccountName;

            this.httpClient = new HttpClient();
            this.automationClient = this.GetAutomationManagementClient();

            this.webHooks = new Dictionary<string, string>
            {
                { "CreateIotHub", this._config.CreateIotHubRunbookUrl },
                { "deleteiothub", this._config.DeleteIotHubRunbookUrl } // note the case for this key - there is a discrepancy in our keyvault key name convention
            };
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
            string authToken = this._tokenHelper.GetServicePrincipleToken();
            TokenCloudCredentials credentials = new TokenCloudCredentials(this._config.SubscriptionId, authToken);
            return new AutomationManagementClient(credentials);
        }

        /// <summary>
        /// Trigger a runbook for the given URL
        /// This method builds a very specific request body using configuration and the given parameters
        /// In general, the webhooks passed to this method will create or delete iot hubs
        /// </summary>
        /// <param name="webHookUrlKey" type="string">The config key for the url for the runbook to trigger</param>
        /// <param name="tenantId" type="string">Tenant Guid</param>
        /// <param name="iotHubName" type="string">Iot Hub Name for deletion or creation</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> TriggerTenantRunbook(string webHookUrlKey, string tenantId, string iotHubName)
        {
            var requestBody = new
            {
                tenantId = tenantId,
                iotHubName = iotHubName,
                token = this._tokenHelper.GetServicePrincipleToken(),
                resourceGroup = this.resourceGroup,
                location = this._config.Location,
                subscriptionId = this._config.SubscriptionId,
                telemetryEventHubConnString = this._config.TelemetryEventHubConnectionString,
                twinChangeEventHubConnString = this._config.TwinChangeEventHubConnectionString,
                lifecycleEventHubConnString = this._config.LifecycleEventHubConnectionString,
                appConfigConnectionString = this._config.AppConfigConnectionString,
                setAppConfigEndpoint = this._config.AppConfigEndpoint
            };

            try
            {
                string webHookUrl = "";
                webHookUrl = this.webHooks[webHookUrlKey];
                if (String.IsNullOrEmpty(webHookUrlKey))
                {
                    throw new Exception($"The requested webhook url {webHookUrlKey} was null. It may not be configured correctly.");
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