// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    public class StatusService : IStatusService
    {
        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private const string AUTH_NAME = "Auth";

        private readonly int timeoutMS = 10000;
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IKeyValueContainer _keyValueContainer;
        private readonly AppConfig config;

        public StatusService(
            ILogger<StatusService> logger,
            IHttpClient httpClient,
            IKeyValueContainer keyValueContainer,
            AppConfig config
            )
        {
            _logger = logger;
            this.config = config;
            _keyValueContainer = keyValueContainer;
            _httpClient = httpClient;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // Check connection to CosmosDb
            var storageResult = await this._keyValueContainer.StatusAsync();
            SetServiceStatus("Storage", storageResult, result, errors);

            if (config.Global.AuthRequired)
            {
                // Check access to Auth
                var authResult = await this.PingServiceAsync(
                    AUTH_NAME,
                    config.ExternalDependencies.AuthServiceUrl);
                SetServiceStatus(AUTH_NAME, authResult, result, errors);
                result.Properties.Add("UserManagementApiUrl", config.ExternalDependencies.AuthServiceUrl);
            }

            result.Properties.Add("StorageType", config.StorageAdapterService.StorageType);
            result.Properties.Add("AuthRequired", config.Global.AuthRequired.ToString());
            result.Properties.Add("Endpoint", config.ASPNETCORE_URLS);
            
            _logger.LogInformation("Service status request {result}", result);

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
            List<string> errors
            )
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
                var response = await this._httpClient.GetAsync(this.PrepareRequest($"{serviceURL}/status"));
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
                _logger.LogError(e, result.Message);
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
                request.Options.AllowInsecureSSLServer = ALLOW_INSECURE_SSL_SERVER;
            }

            _logger.LogDebug("Prepare request {request}", request);

            return request;
        }

    }
}
