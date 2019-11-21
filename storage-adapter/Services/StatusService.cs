// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Mmm.Platform.IoT.StorageAdapter.Services.Runtime;
using Mmm.Platform.IoT.Common.Services;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    class StatusService : IStatusService
    {
        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private const string AUTH_NAME = "Auth";

        private readonly int timeoutMS = 10000;
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IKeyValueContainer _keyValueContainer;
        private readonly IServicesConfig _servicesConfig;

        public StatusService(
            ILogger<StatusService> logger,
            IHttpClient httpClient,
            IKeyValueContainer keyValueContainer,
            IServicesConfig servicesConfig
            )
        {
            _logger = logger;
            this._keyValueContainer = keyValueContainer;
            this._servicesConfig = servicesConfig;
            this._httpClient = httpClient;
        }

        public async Task<StatusServiceModel> GetStatusAsync(bool authRequired)
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // Check connection to CosmosDb
            var storageResult = await this._keyValueContainer.PingAsync();
            SetServiceStatus("Storage", storageResult, result, errors);

            if (this._servicesConfig.AuthRequired)
            {
                // Check access to Auth
                var authResult = await this.PingServiceAsync(
                    AUTH_NAME,
                    this._servicesConfig.UserManagementApiUrl);
                SetServiceStatus(AUTH_NAME, authResult, result, errors);
                result.Properties.Add("UserManagementApiUrl", this._servicesConfig?.UserManagementApiUrl);
            }

            result.Properties.Add("StorageType", this._servicesConfig.StorageType);
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
