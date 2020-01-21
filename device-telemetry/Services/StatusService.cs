using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services
{
    public class StatusService : IStatusService
    {
        private const string STORAGE_TYPE_KEY = "StorageType";
        private const string TIME_SERIES_KEY = "tsi";
        private const string TIME_SERIES_EXPLORER_URL_KEY = "TsiExplorerUrl";
        private const string TIME_SERIES_EXPLORER_URL_SEPARATOR_CHAR = ".";

        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private readonly int timeoutMS = 10000;

        private readonly IStorageClient storageClient;
        private readonly ITimeSeriesClient timeSeriesClient;
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;
        private readonly AppConfig config;

        public StatusService(
            ILogger<StatusService> logger,
            IStorageClient storageClient,
            ITimeSeriesClient timeSeriesClient,
            IHttpClient httpClient,
            AppConfig config)
        {
            this.logger = logger;
            this.storageClient = storageClient;
            this.timeSeriesClient = timeSeriesClient;
            this.httpClient = httpClient;
            this.config = config;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();
            var explorerUrl = string.Empty;

            string storageAdapterName = "StorageAdapter";
            string storageName = "Storage";
            string diagnosticsName = "Diagnostics";
            string authName = "Auth";
            string timeSeriesName = "TimeSeries";
            string asaManagerName = "AsaManager";

            var asaManagerResult = await this.PingServiceAsync(
                asaManagerName,
                this.config.ExternalDependencies.AsaManagerServiceUrl);
            SetServiceStatus(asaManagerName, asaManagerResult, result, errors);

            // Check access to StorageAdapter
            var storageAdapterResult = await this.PingServiceAsync(
                storageAdapterName,
                this.config.ExternalDependencies.StorageAdapterServiceUrl);
            SetServiceStatus(storageAdapterName, storageAdapterResult, result, errors);

            if (config.Global.AuthRequired)
            {
                // Check access to Auth
                var authResult = await this.PingServiceAsync(
                    authName,
                    this.config.ExternalDependencies.AuthServiceUrl);
                SetServiceStatus(authName, authResult, result, errors);
                result.Properties.Add("UserManagementApiUrl", this.config?.ExternalDependencies.AuthServiceUrl);
            }

            // Check access to Diagnostics
            var diagnosticsResult = await this.PingServiceAsync(
                diagnosticsName,
                this.config.ExternalDependencies.DiagnosticsServiceUrl);
            // Note: Overall simulation service status is independent of diagnostics service
            // Hence not using SetServiceStatus on diagnosticsResult
            result.Dependencies.Add(diagnosticsName, diagnosticsResult);

            // Add Time Series Dependencies if needed
            if (this.config.DeviceTelemetryService.Messages.TelemetryStorageType.Equals(
                TIME_SERIES_KEY,
                StringComparison.OrdinalIgnoreCase))
            {
                // Check connection to Time Series Insights
                var timeSeriesResult = await this.timeSeriesClient.StatusAsync();
                SetServiceStatus(timeSeriesName, timeSeriesResult, result, errors);

                // Add Time Series Insights explorer url
                var timeSeriesFqdn = this.config.DeviceTelemetryService.TimeSeries.TsiDataAccessFqdn;
                var environmentId = timeSeriesFqdn.Substring(0, timeSeriesFqdn.IndexOf(TIME_SERIES_EXPLORER_URL_SEPARATOR_CHAR));
                explorerUrl = this.config.DeviceTelemetryService.TimeSeries.ExplorerUrl +
                    "?environmentId=" + environmentId +
                    "&tid=" + this.config.Global.AzureActiveDirectory.TenantId;
                result.Properties.Add(TIME_SERIES_EXPLORER_URL_KEY, explorerUrl);
            }

            // Check access to Storage
            var storageResult = await this.storageClient.StatusAsync();
            SetServiceStatus(storageName, storageResult, result, errors);

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }

            result.Properties.Add("DiagnosticsEndpointUrl", this.config?.ExternalDependencies.DiagnosticsServiceUrl);
            result.Properties.Add("StorageAdapterApiUrl", this.config?.ExternalDependencies.StorageAdapterServiceUrl);
            result.Properties.Add("AuthRequired", config.Global.AuthRequired.ToString());
            result.Properties.Add("Endpoint", config.ASPNETCORE_URLS);

            logger.LogInformation("Service status request {result}", result);

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
                if (response.IsError)
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
            request.AddHeader(HttpRequestHeader.Referer.ToString(), "Device Telemetry " + this.GetType().FullName);
            request.SetUriFromString(path);
            request.Options.EnsureSuccess = false;
            request.Options.Timeout = this.timeoutMS;
            if (path.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = ALLOW_INSECURE_SSL_SERVER;
            }

            logger.LogDebug("Prepare request {request}", request);

            return request;
        }
    }
}
