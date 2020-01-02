using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Mmm.Platform.IoT.TenantManager.Services.Runtime;
using Mmm.Platform.IoT.TenantManager.Services.Exceptions;
using Microsoft.Azure;
using Microsoft.Azure.Management.Automation;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services.Helpers
{
    public class RunbookHelper : IRunbookHelper
    {
        private const string SA_JOB_DATABASE_ID = "pcs-iothub-stream";

        private string iotHubConnectionStringKeyFormat = "tenant:{0}:iotHubConnectionString";
        private Regex iotHubKeyRegexMatch = new Regex(@"(?<=SharedAccessKey=)[^;]*");
        private Regex storageAccountKeyRegexMatch = new Regex(@"(?<=AccountKey=)[^;]*");

        // injection variables
        private readonly IServicesConfig _config;
        private readonly ITokenHelper _tokenHelper;
        private readonly IAppConfigurationHelper _appConfigHelper;

        public HttpClient httpClient;

        public RunbookHelper(IServicesConfig config, ITokenHelper tokenHelper, IAppConfigurationHelper appConfigHelper)
        {
            this._tokenHelper = tokenHelper;
            this._config = config;
            this._appConfigHelper = appConfigHelper;

            this.httpClient = new HttpClient();
        }

        /// <summary>
        /// Get an automation client using an auth token from the token helper
        /// </summary>
        /// <returns>AutomationManagementClient</returns>
        private async Task<AutomationManagementClient> GetAutomationClientAsync()
        {
            string authToken = await this._tokenHelper.GetTokenAsync();
            TokenCloudCredentials credentials = new TokenCloudCredentials(this._config.SubscriptionId, authToken);
            return new AutomationManagementClient(credentials);
        }

        private string GetRegexMatch(string matchString, Regex expression)
        {

            Match match = expression.Match(matchString);
            string value = match.Value;
            if (String.IsNullOrEmpty(value))
            {
                throw new Exception($"Unable to match a value from string {matchString} for the given regular expression {expression.ToString()}");
            }
            return value;
        }

        private string GetIotHubKey(string tenantId, string iotHubName)
        {
            try
            {
                string appConfigKey = String.Format(this.iotHubConnectionStringKeyFormat, tenantId);
                string iotHubConnectionString = this._appConfigHelper.GetValue(appConfigKey);
                if (String.IsNullOrEmpty(iotHubConnectionString))
                {
                    throw new Exception($"The iotHubConnectionString returned by app config for the key {appConfigKey} returned a null value.");
                }
                return this.GetRegexMatch(iotHubConnectionString, this.iotHubKeyRegexMatch);
            }
            catch (Exception e)
            {
                throw new IotHubKeyException($"Unable to get the iothub SharedAccessKey for tenant {tenantId}", e);
            }
        }

        private string GetStorageAccountKey(string connectionString)
        {
            try
            {
                return this.GetRegexMatch(connectionString, this.storageAccountKeyRegexMatch);
            }
            catch (Exception e)
            {
                throw new StorageAccountKeyException("Unable to get the Storage Account Key from the connection string. The connection string may not be configured correctly.", e);
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
                "CreateIotHub",
                "DeleteIotHub",
                "CreateSAJob",
                "DeleteSAJob"
            };
            foreach (var webHook in webHooks)
            {
                try
                {
                    var automationClient = await this.GetAutomationClientAsync();
                    var webHookResponse = await automationClient.Webhooks.GetAsync(this._config.ResourceGroup, this._config.AutomationAccountName, webHook);
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
            return await this.TriggerIotHubRunbook(this._config.CreateIotHubRunbookUrl, tenantId, iotHubName, dpsName);
        }

        public async Task<HttpResponseMessage> DeleteIotHub(string tenantId, string iotHubName, string dpsName)
        {
            return await this.TriggerIotHubRunbook(this._config.DeleteIotHubRunbookUrl, tenantId, iotHubName, dpsName);
        }

        public async Task<HttpResponseMessage> CreateAlerting(string tenantId, string saJobName, string iotHubName)
        {
            string iotHubKey = this.GetIotHubKey(tenantId, iotHubName);
            string storageAccountKey = this.GetStorageAccountKey(this._config.StorageAccountConnectionString);

            var requestBody = new
            {
                tenantId = tenantId,
                location = this._config.Location,
                resourceGroup = this._config.ResourceGroup,
                subscriptionId = this._config.SubscriptionId,
                saJobName = saJobName,
                storageAccountName = this._config.StorageAccountName,
                storageAccountKey = storageAccountKey,
                eventHubNamespaceName = this._config.EventHubNamespaceName,
                eventHubAccessPolicyKey = this._config.EventHubAccessPolicyKey,
                iotHubName = iotHubName,
                iotHubAccessKey = iotHubKey,
                cosmosDbAccountName = this._config.CosmosDbAccount,
                cosmosDbAccountKey = this._config.CosmosDbKey,
                cosmosDbDatabaseId = SA_JOB_DATABASE_ID
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            return await this.TriggerRunbook(this._config.CreateStreamAnalyticsRunbookUrl, bodyContent);
        }

        public async Task<HttpResponseMessage> DeleteAlerting(string tenantId, string saJobName)
        {
            var requestBody = new
            {
                tenantId = tenantId,
                resourceGroup = this._config.ResourceGroup,
                subscriptionId = this._config.SubscriptionId,
                saJobName = saJobName,
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            return await this.TriggerRunbook(this._config.DeleteStreamAnalyticsRunbookUrl, bodyContent);
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
        private async Task<HttpResponseMessage> TriggerIotHubRunbook(string webHookUrl, string tenantId, string iotHubName, string dpsName)
        {
            var requestBody = new
            {
                tenantId = tenantId,
                iotHubName = iotHubName,
                dpsName = dpsName,
                token = await this._tokenHelper.GetTokenAsync(),
                resourceGroup = this._config.ResourceGroup,
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

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            return await this.TriggerRunbook(webHookUrl, bodyContent);
        }

        private async Task<HttpResponseMessage> TriggerRunbook(string webHookUrl, StringContent bodyContent)
        {
            try
            {
                if (String.IsNullOrEmpty(webHookUrl))
                {
                    throw new Exception($"The given webHookUrl string was null or empty. It may not be configured correctly.");
                }
                return await this.httpClient.PostAsync(webHookUrl, bodyContent);
            }
            catch (Exception e)
            {
                throw new RunbookTriggerException($"Unable to successfully trigger the requested runbook operation.", e);
            }
        }
    }
}