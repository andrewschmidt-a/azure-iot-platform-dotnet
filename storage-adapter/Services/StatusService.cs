// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Http;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    class StatusService : IStatusService
    {
        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private const string AUTH_NAME = "Auth";

        private readonly int timeoutMS = 10000;
        private readonly ILogger _log;
        private readonly IHttpClient _httpClient;
        private readonly IKeyValueContainer _keyValueContainer;
        private readonly IServicesConfig _servicesConfig;

        public StatusService(
            ILogger logger,
            IHttpClient httpClient,
            IKeyValueContainer keyValueContainer,
            IServicesConfig servicesConfig
            )
        {
            this._log = logger;
            this._keyValueContainer = keyValueContainer;
            this._servicesConfig = servicesConfig;
            this._httpClient = httpClient;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // Check connection to CosmosDb
            var storageResult = await this._keyValueContainer.PingAsync();
            SetServiceStatus("Storage", storageResult, result, errors);
            
            result.Properties.Add("StorageType", this._servicesConfig.StorageType);
            this._log.Info(
                "Service status request",
                () => new
                {
                    Healthy = result.Status.IsHealthy,
                    result.Status.Message
                });

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
                this._log.Error(result.Message, () => new { e });
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

            this._log.Debug("Prepare Request", () => new { request });

            return request;
        }

    }
}
