// <copyright file="RunbookHelper.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Management.Automation;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.TenantManager.Services.Exceptions;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.TenantManager.Services.Helpers
{
    public class RunbookHelper : IRunbookHelper, IDisposable
    {
        private const string SaJobDatabaseId = "pcs-iothub-stream";
        private readonly AppConfig config;
        private readonly ITokenHelper tokenHelper;
        private readonly IAppConfigurationClient appConfigClient;
        private HttpClient httpClient;
        private bool disposedValue = false;
        private string iotHubConnectionStringKeyFormat = "tenant:{0}:iotHubConnectionString";
        private Regex iotHubKeyRegexMatch = new Regex(@"(?<=SharedAccessKey=)[^;]*");
        private Regex storageAccountKeyRegexMatch = new Regex(@"(?<=AccountKey=)[^;]*");

        public RunbookHelper(AppConfig config, ITokenHelper tokenHelper, IAppConfigurationClient appConfigHelper)
        {
            this.tokenHelper = tokenHelper;
            this.config = config;
            this.appConfigClient = appConfigHelper;

            this.httpClient = new HttpClient();
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            string unhealthyMessage = string.Empty;
            List<string> webHooks = new List<string>
            {
                "CreateIotHub",
                "DeleteIotHub",
                "CreateSAJob",
                "DeleteSAJob",
            };
            foreach (var webHook in webHooks)
            {
                try
                {
                    var automationClient = await this.GetAutomationClientAsync();
                    var webHookResponse = await automationClient.Webhooks.GetAsync(config.Global.ResourceGroup, config.TenantManagerService.AutomationAccountName, webHook);
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

            return string.IsNullOrEmpty(unhealthyMessage) ? new StatusResultServiceModel(true, "Alive and well!") : new StatusResultServiceModel(false, unhealthyMessage);
        }

        public async Task<HttpResponseMessage> CreateIotHub(string tenantId, string iotHubName, string dpsName)
        {
            return await this.TriggerIotHubRunbook(config.TenantManagerService.CreateIotHubWebHookUrl, tenantId, iotHubName, dpsName);
        }

        public async Task<HttpResponseMessage> DeleteIotHub(string tenantId, string iotHubName, string dpsName)
        {
            return await this.TriggerIotHubRunbook(config.TenantManagerService.DeleteIotHubWebHookUrl, tenantId, iotHubName, dpsName);
        }

        public async Task<HttpResponseMessage> CreateAlerting(string tenantId, string saJobName, string iotHubName)
        {
            string iotHubKey = this.GetIotHubKey(tenantId, iotHubName);
            string storageAccountKey = this.GetStorageAccountKey(config.Global.StorageAccountConnectionString);

            var requestBody = new
            {
                tenantId = tenantId,
                location = config.Global.Location,
                resourceGroup = config.Global.ResourceGroup,
                subscriptionId = config.Global.SubscriptionId,
                saJobName = saJobName,
                storageAccountName = config.Global.StorageAccount.Name,
                storageAccountKey = storageAccountKey,
                eventHubNamespaceName = config.TenantManagerService.EventHubNamespaceName,
                eventHubAccessPolicyKey = config.TenantManagerService.EventHubAccessPolicyKey,
                iotHubName = iotHubName,
                iotHubAccessKey = iotHubKey,
                cosmosDbAccountName = config.Global.CosmosDb.AccountName,
                cosmosDbAccountKey = config.Global.CosmosDb.DocumentDbAuthKey,
                cosmosDbDatabaseId = SaJobDatabaseId,
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            return await this.TriggerRunbook(config.TenantManagerService.CreateSaJobWebHookUrl, bodyContent);
        }

        public async Task<HttpResponseMessage> DeleteAlerting(string tenantId, string saJobName)
        {
            var requestBody = new
            {
                tenantId = tenantId,
                resourceGroup = config.Global.ResourceGroup,
                subscriptionId = config.Global.SubscriptionId,
                saJobName = saJobName,
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            return await this.TriggerRunbook(config.TenantManagerService.DeleteSaJobWebHookUrl, bodyContent);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    httpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        private async Task<HttpResponseMessage> TriggerIotHubRunbook(string webHookUrl, string tenantId, string iotHubName, string dpsName)
        {
            var requestBody = new
            {
                tenantId = tenantId,
                iotHubName = iotHubName,
                dpsName = dpsName,
                token = await this.tokenHelper.GetTokenAsync(),
                resourceGroup = config.Global.ResourceGroup,
                location = config.Global.Location,
                subscriptionId = config.Global.SubscriptionId,

                // Event Hub Connection Strings for setting up IoT Hub Routing
                telemetryEventHubConnString = config.TenantManagerService.TelemetryEventHubConnectionString,
                twinChangeEventHubConnString = config.TenantManagerService.TwinChangeEventHubConnectionString,
                lifecycleEventHubConnString = config.TenantManagerService.LifecycleEventHubConnectionString,
                appConfigConnectionString = config.AppConfigurationConnectionString,
                setAppConfigEndpoint = config.TenantManagerService.SetAppConfigEndpoint,
                storageAccount = config.Global.StorageAccount.Name,
            };

            var bodyContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            return await this.TriggerRunbook(webHookUrl, bodyContent);
        }

        private async Task<HttpResponseMessage> TriggerRunbook(string webHookUrl, StringContent bodyContent)
        {
            try
            {
                if (string.IsNullOrEmpty(webHookUrl))
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

        private async Task<AutomationManagementClient> GetAutomationClientAsync()
        {
            string authToken = await this.tokenHelper.GetTokenAsync();
            TokenCloudCredentials credentials = new TokenCloudCredentials(config.Global.SubscriptionId, authToken);
            return new AutomationManagementClient(credentials);
        }

        private string GetRegexMatch(string matchString, Regex expression)
        {
            Match match = expression.Match(matchString);
            string value = match.Value;
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception($"Unable to match a value from string {matchString} for the given regular expression {expression.ToString()}");
            }

            return value;
        }

        private string GetIotHubKey(string tenantId, string iotHubName)
        {
            try
            {
                string appConfigKey = string.Format(this.iotHubConnectionStringKeyFormat, tenantId);
                string iotHubConnectionString = this.appConfigClient.GetValue(appConfigKey);
                if (string.IsNullOrEmpty(iotHubConnectionString))
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
    }
}