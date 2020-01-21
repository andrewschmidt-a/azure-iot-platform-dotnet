using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    public class StatusService : IStatusService
    {
        private const bool AllowInsecureSslServer = true;
        private const string AuthName = "Auth";
        private readonly int timeoutMS = 10000;
        private readonly ILogger logger;
        private readonly IHttpClient httpClient;
        private readonly IKeyValueContainer keyValueContainer;
        private readonly AppConfig config;

        public StatusService(
            ILogger<StatusService> logger,
            IHttpClient httpClient,
            IKeyValueContainer keyValueContainer,
            AppConfig config)
        {
            this.logger = logger;
            this.config = config;
            this.keyValueContainer = keyValueContainer;
            this.httpClient = httpClient;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // Check connection to CosmosDb
            var storageResult = await this.keyValueContainer.StatusAsync();
            SetServiceStatus("Storage", storageResult, result, errors);

            if (config.Global.AuthRequired)
            {
                // Check access to Auth
                var authResult = await this.PingServiceAsync(
                    AuthName,
                    config.ExternalDependencies.AuthServiceUrl);
                SetServiceStatus(AuthName, authResult, result, errors);
                result.Properties.Add("UserManagementApiUrl", config.ExternalDependencies.AuthServiceUrl);
            }

            result.Properties.Add("StorageType", config.StorageAdapterService.StorageType);
            result.Properties.Add("AuthRequired", config.Global.AuthRequired.ToString());
            result.Properties.Add("Endpoint", config.ASPNETCORE_URLS);

            logger.LogInformation("Service status request {result}", result);

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }

            return result;
        }

        private void SetServiceStatus(
            string dependencyName,
            StatusResultServiceModel serviceResult,
            StatusServiceModel result,
            List<string> errors)
        {
            if (!serviceResult.IsHealthy)
            {
                errors.Add(dependencyName + " check failed");
                result.Status.IsHealthy = false;
            }

            result.Dependencies.Add(dependencyName, serviceResult);
        }

        private async Task<StatusResultServiceModel> PingServiceAsync(string serviceName, string serviceURL)
        {
            var result = new StatusResultServiceModel(false, $"{serviceName} check failed");
            try
            {
                var response = await this.httpClient.GetAsync(this.PrepareRequest($"{serviceURL}/status"));
                if (!response.IsSuccessStatusCode)
                {
                    result.Message = $"Status code: {response.StatusCode}; Response: {response.Content}";
                }
                else
                {
                    var data = JsonConvert.DeserializeObject<StatusServiceModel>(response.Content);
                    result = data.Status;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, result.Message);
            }

            return result;
        }

        private HttpRequest PrepareRequest(string path)
        {
            var request = new HttpRequest();
            request.AddHeader(HttpRequestHeader.Accept.ToString(), "application/json");
            request.AddHeader(HttpRequestHeader.CacheControl.ToString(), "no-cache");
            request.AddHeader(HttpRequestHeader.Referer.ToString(), "StorageAdapter " + this.GetType().FullName);
            request.SetUriFromString(path);
            request.Options.EnsureSuccess = false;
            request.Options.Timeout = this.timeoutMS;
            if (path.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = AllowInsecureSslServer;
            }

            logger.LogDebug("Prepare request {request}", request);

            return request;
        }
    }
}