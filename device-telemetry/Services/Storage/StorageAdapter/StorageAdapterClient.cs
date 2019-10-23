// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.StorageAdapter;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.StorageAdapter
{
    public interface IStorageAdapterClient
    {
        Task<ValueListApiModel> GetAllAsync(string collectionId);
        Task<ValueApiModel> GetAsync(string collectionId, string key);
        Task<ValueApiModel> CreateAsync(string collectionId, string value);
        Task<ValueApiModel> UpsertAsync(string collectionId, string key, string value, string etag);
        Task DeleteAsync(string collectionId, string key);
    }

    // TODO: handle retriable errors
    public class StorageAdapterClient : IStorageAdapterClient
    {
        // TODO: make it configurable, default to false
        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string AZDS_ROUTE_KEY = "azds-route-as";

        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;
        private readonly int timeout;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StorageAdapterClient(
            IHttpClient httpClient,
            IServicesConfig config,
            ILogger logger,
            IHttpContextAccessor contextAccessor)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.StorageAdapterApiUrl;
            this.timeout = config.StorageAdapterApiTimeout;
            this._httpContextAccessor = contextAccessor;
        }

        public async Task<ValueListApiModel> GetAllAsync(string collectionId)
        {
            var response = await this.httpClient.GetAsync(
                this.PrepareRequest($"collections/{collectionId}/values"));

            this.ThrowIfError(response, collectionId, "");

            return JsonConvert.DeserializeObject<ValueListApiModel>(response.Content);
        }

        public async Task<ValueApiModel> GetAsync(string collectionId, string key)
        {
            var response = await this.httpClient.GetAsync(
                this.PrepareRequest($"collections/{collectionId}/values/{key}"));

            this.ThrowIfError(response, collectionId, key);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task<ValueApiModel> CreateAsync(string collectionId, string value)
        {
            var response = await this.httpClient.PostAsync(
                this.PrepareRequest($"collections/{collectionId}/values",
                    new ValueApiModel { Data = value }));

            this.ThrowIfError(response, collectionId, "");

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task<ValueApiModel> UpsertAsync(string collectionId, string key, string value, string etag)
        {
            var response = await this.httpClient.PutAsync(
                this.PrepareRequest($"collections/{collectionId}/values/{key}",
                    new ValueApiModel { Data = value, ETag = etag }));

            this.ThrowIfError(response, collectionId, key);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
            var response = await this.httpClient.DeleteAsync(
                this.PrepareRequest($"collections/{collectionId}/values/{key}"));

            this.ThrowIfError(response, collectionId, key);
        }

        private Http.HttpRequest PrepareRequest(string path, ValueApiModel content = null)
        {
            string tenantId = null;
            if(this._httpContextAccessor.HttpContext != null){
                tenantId = this._httpContextAccessor.HttpContext.Request.GetTenant();
            }else{
                throw new Exception("No tenant Found");
            }
            
            var request = new Http.HttpRequest();
            request.AddHeader(HttpRequestHeader.Accept.ToString(), "application/json");
            request.AddHeader(HttpRequestHeader.CacheControl.ToString(), "no-cache");
            request.AddHeader(HttpRequestHeader.UserAgent.ToString(), "Device Simulation " + this.GetType().FullName);
            request.Headers.Add(TENANT_HEADER, tenantId);
            
            // Add Azds route if exists
            if (this._httpContextAccessor.HttpContext.Request.Headers.ContainsKey(AZDS_ROUTE_KEY))
            {
                try
                {
                    var azdsRouteAs = this._httpContextAccessor.HttpContext.Request.Headers.First(p => String.Equals(p.Key, AZDS_ROUTE_KEY, StringComparison.OrdinalIgnoreCase));
                    request.Headers.Add(AZDS_ROUTE_KEY, azdsRouteAs.Value.First());  // azdsRouteAs.Value returns an iterable of strings, take the first
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to attach the {AZDS_ROUTE_KEY} header to the StorageAdapterClient Request.", e);
                }
            }
            
            request.SetUriFromString($"{this.serviceUri}/{path}");
            request.Options.EnsureSuccess = false;
            request.Options.Timeout = this.timeout;
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = ALLOW_INSECURE_SSL_SERVER;
            }

            if (content != null)
            {
                request.SetContent(content);
            }

            return request;
        }

        private void ThrowIfError(IHttpResponse response, string collectionId, string key)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ResourceNotFoundException($"Resource {collectionId}/{key} not found.");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConflictingResourceException(
                    $"Resource {collectionId}/{key} out of date. Reload the resource and retry.");
            }

            if (response.IsError)
            {
                throw new ExternalDependencyException(
                    new HttpRequestException($"Storage request error: status code {response.StatusCode}"));
            }
        }
    }
}
