using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Mmm.Platform.IoT.TenantManager.Services.Runtime;
using Mmm.Platform.IoT.TenantManager.Services.Exceptions;
using Mmm.Platform.IoT.TenantManager.Services.Models;
using Microsoft.Azure;
using Microsoft.Azure.Management.Automation;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services.Helpers
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
        }

        /// <summary>
        /// Create the automation client using the tokenHelper and config variables to authenticate
        /// </summary>
        /// <returns>AutomationManagementClient</returns>
        private AutomationManagementClient automationClient
        {
            get
            {
                string authToken = this._tokenHelper.GetServicePrincipleToken();
                TokenCloudCredentials credentials = new TokenCloudCredentials(this._config.SubscriptionId, authToken);
                return new AutomationManagementClient(credentials);
            }
        }

        /// <summary>
        /// Return the status of the create and delete runbooks
        /// </summary>
        /// <returns>StatusResultServiceModel task</returns>
        public async Task<StatusResultServiceModel> StatusAsync()
        {
            string unhealthyMessage = "";
            List<string> webHooks = new List<string>
            {
                this._config.CreateIotHubRunbookName,
                this._config.DeleteIotHubRunbookName
            };
            foreach (var webHook in webHooks)
            {
                try
                {
                    var webHookResponse = await this.automationClient.Webhooks.GetAsync(this.resourceGroup, this.automationAccountName, webHook);
                    if (!webHookResponse.Webhook.Properties.IsEnabled)
                    {
                        unhealthyMessage += $"{webHook} is not enabled.\n";
                    }
                }
                catch (Exception e)
                {
                    unhealthyMessage += $"Unable to get status for {webHook}: {e.Message}";
                }
            }
            return String.IsNullOrEmpty(unhealthyMessage) ? new StatusResultServiceModel(true, "Alive and well!") : new StatusResultServiceModel(false, unhealthyMessage);
        }

        public async Task<HttpResponseMessage> CreateIotHub(string tenantId, string iotHubName, string dpsName)
        {
            return await this.TriggerTenantRunbook(this._config.CreateIotHubRunbookUrl, tenantId, iotHubName, dpsName);
        }

        public async Task<HttpResponseMessage> DeleteIotHub(string tenantId, string iotHubName, string dpsName)
        {
            return await this.TriggerTenantRunbook(this._config.DeleteIotHubRunbookUrl, tenantId, iotHubName, dpsName);
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
        private async Task<HttpResponseMessage> TriggerTenantRunbook(string webHookUrl, string tenantId, string iotHubName, string dpsName)
        {
            var requestBody = new
            {
                tenantId = tenantId,
                iotHubName = iotHubName,
                dpsName = dpsName,
                token = this._tokenHelper.GetServicePrincipleToken(),
                resourceGroup = this.resourceGroup,
                location = this._config.Location,
                subscriptionId = this._config.SubscriptionId,
                // Event Hub Connection Strings for setting up IoT Hub Routing
                telemetryEventHubConnString = this._config.TelemetryEventHubConnectionString,
                twinChangeEventHubConnString = this._config.TwinChangeEventHubConnectionString,
                lifecycleEventHubConnString = this._config.LifecycleEventHubConnectionString,
                appConfigConnectionString = this._config.ApplicationConfigurationConnectionString,
                setAppConfigEndpoint = this._config.AppConfigEndpoint,
                storageAccount = this._config.StorageAccountName
            };

            try
            {
                if (String.IsNullOrEmpty(webHookUrl))
                {
                    throw new Exception($"The given webHookUrl string was null or empty. It may not be configured correctly.");
                }
                var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                return await this.httpClient.PostAsync(webHookUrl, bodyContent);
            }
            catch (Exception e)
            {
                throw new RunbookTriggerException($"Unable to successfully trigger the requested runbook operation.", e);
            }
        }
    }
}