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

namespace Mmm.Platform.IoT.Config.Services
{
    public class StatusService : IStatusService
    {
        private readonly ILogger _logger;
        private readonly IHttpClient httpClient;
        private readonly AppConfig config;
        private readonly int timeoutMS = 10000;

        private const bool ALLOW_INSECURE_SSL_SERVER = true;

        public StatusService(
            ILogger<StatusService> logger,
            IHttpClient httpClient,
            AppConfig config)
        {
            _logger = logger;
            this.httpClient = httpClient;
            this.config = config;
        }

        public async Task<StatusServiceModel> GetStatusAsync(bool authRequired)
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();
            string storageAdapterName = "StorageAdapter";
            string deviceTelemetryName = "DeviceTelemetry";
            string deviceSimulationName = "DeviceSimulation";
            string authName = "Auth";
            string asaManagerName = "AsaManager";

            var asaManagerResult = await this.PingServiceAsync(
                asaManagerName,
                config.ExternalDependencies.AsaManagerServiceUrl);
            SetServiceStatus(asaManagerName, asaManagerResult, result, errors);

            // Check access to StorageAdapter
            var storageAdapterResult = await this.PingServiceAsync(
                storageAdapterName,
                config.ExternalDependencies.StorageAdapterServiceUrl);
            SetServiceStatus(storageAdapterName, storageAdapterResult, result, errors);

            // Check access to Device Telemetry
            var deviceTelemetryResult = await this.PingServiceAsync(
                deviceTelemetryName,
                config.ExternalDependencies.TelemetryServiceUrl);
            SetServiceStatus(deviceTelemetryName, deviceTelemetryResult, result, errors);

            // Check access to DeviceSimulation

            /* TODO: Remove PingSimulationAsync and use PingServiceAsync once DeviceSimulation has started 
             * using the new 'Status' model */
            var deviceSimulationResult = await this.PingSimulationAsync(
                deviceSimulationName,
                config.ExternalDependencies.DeviceSimulationServiceUrl);

            // Andrew Schmidt -- disabling until we stand up simulation
            // SetServiceStatus(deviceSimulationName, deviceSimulationResult, result, errors);

            var authResult = await this.PingServiceAsync(
                authName,
                config.ExternalDependencies.AuthServiceUrl);
            SetServiceStatus(authName, authResult, result, errors);

            // Add properties
            result.Properties.Add("DeviceSimulationApiUrl", config?.ExternalDependencies.DeviceSimulationServiceUrl);
            result.Properties.Add("StorageAdapterApiUrl", config?.ExternalDependencies.StorageAdapterServiceUrl);
            result.Properties.Add("UserManagementApiUrl", config?.ExternalDependencies.AuthServiceUrl);
            result.Properties.Add("TelemetryApiUrl", config?.ExternalDependencies.TelemetryServiceUrl);
            result.Properties.Add("SeedTemplate", config?.ConfigService.SeedTemplate);
            result.Properties.Add("SolutionType", config?.ConfigService.SolutionType);

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

        private async Task<StatusResultServiceModel> PingSimulationAsync(string serviceName, string serviceURL)
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
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                    result.Message = data["Status"].ToString();
                    result.IsHealthy = data["Status"].ToString().StartsWith("OK:");
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
            request.AddHeader(HttpRequestHeader.Referer.ToString(), "Config " + this.GetType().FullName);
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
