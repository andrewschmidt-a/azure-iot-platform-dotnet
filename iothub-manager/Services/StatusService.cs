// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public class StatusService : IStatusService
    {
        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private readonly int timeoutMS = 10000;

        private readonly IDevices devices;
        private readonly IHttpClient httpClient;
        private readonly ILogger _logger;
        private readonly AppConfig config;

        public StatusService(
            ILogger<StatusService> logger,
            IHttpClient httpClient,
            IDevices devices,
            AppConfig config)
        {
            _logger = logger;
            this.httpClient = httpClient;
            this.devices = devices;
            this.config = config;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            string storageAdapterName = "StorageAdapter";
            string authName = "Auth";


            // Check access to StorageAdapter
            var storageAdapterResult = await this.PingServiceAsync(
                storageAdapterName,
                config.ExternalDependencies.StorageAdapterServiceUrl);
            SetServiceStatus(storageAdapterName, storageAdapterResult, result, errors);

            if (config.Global.AuthRequired)
            {
                // Check access to Auth
                var authResult = await this.PingServiceAsync(
                    authName,
                    config.ExternalDependencies.AuthServiceUrl);
                SetServiceStatus(authName, authResult, result, errors);
                result.Properties.Add("UserManagementApiUrl", config?.ExternalDependencies.AuthServiceUrl);
            }

            // Preprovisioned IoT hub status
            var isHubPreprovisioned = this.IsHubConnectionStringConfigured();

            if (isHubPreprovisioned)
            {
                var ioTHubResult = await this.devices.PingRegistryAsync();
                SetServiceStatus("IoTHub", ioTHubResult, result, errors);
            }

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }

            result.Properties.Add("StorageAdapterApiUrl", config?.ExternalDependencies.StorageAdapterServiceUrl);
            result.Properties.Add("AuthRequired", config.Global.AuthRequired.ToString());
            result.Properties.Add("Endpoint", config.ASPNETCORE_URLS);
            
            _logger.LogInformation("Service status request {result}", result);

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

        // Check whether the configuration contains a connection string
        private bool IsHubConnectionStringConfigured()
        {
            var cs = config?.AppConfigurationConnectionString?.ToLowerInvariant().Trim();
            return (!string.IsNullOrEmpty(cs)
                    && cs.Contains("hostname=")
                    && cs.Contains("sharedaccesskeyname=")
                    && cs.Contains("sharedaccesskey="));
        }
    }
}
