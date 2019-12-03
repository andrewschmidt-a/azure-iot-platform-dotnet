// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Http;
using Newtonsoft.Json;
using HttpRequest = Mmm.Platform.IoT.Common.Services.Http.HttpRequest;

namespace Mmm.Platform.IoT.Common.Services.External.StorageAdapter
{
    public class StorageAdapterClient : IStorageAdapterClient
    {
        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string TENANT_ID = "TenantID";
        private const string AZDS_ROUTE_KEY = "azds-route-as";

        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int timeout;

        public StorageAdapterClient(IHttpClient httpClient, IStorageAdapterClientConfig storageAdapterClientConfig, ILogger logger, IHttpContextAccessor contextAccessor)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = storageAdapterClientConfig.StorageAdapterApiUrl;
            this._httpContextAccessor = contextAccessor;
            this.timeout = storageAdapterClientConfig.StorageAdapterApiTimeout;
        }

        public async Task<ValueApiModel> UpsertAsync(string collectionId, string key, string value, string etag)
        {
            var response = await this.httpClient.PutAsync(
                this.PrepareRequest($"collections/{collectionId}/values/{key}",
                    new ValueApiModel { Data = value, ETag = etag }));

            this.ThrowIfError(response, collectionId, key);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task<ValueApiModel> CreateAsync(string collectionId, string value)
        {
            var request = this.CreateRequest($"collections/{collectionId}/values", new ValueApiModel
            {
                Data = value
            });
            var response = await this.httpClient.PostAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task<ValueApiModel> GetAsync(string collectionId, string key)
        {
            var request = this.CreateRequest($"collections/{collectionId}/values/{key}");
            var response = await this.httpClient.GetAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task<ValueListApiModel> GetAllAsync(string collectionId)
        {
            var request = this.CreateRequest($"collections/{collectionId}/values");
            var response = await this.httpClient.GetAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueListApiModel>(response.Content);
        }

        public async Task<ValueApiModel> UpdateAsync(string collectionId, string key, string value, string etag)
        {
            var request = this.CreateRequest($"collections/{collectionId}/values/{key}", new ValueApiModel
            {
                Data = value,
                ETag = etag
            });
            var response = await this.httpClient.PutAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
            var request = this.CreateRequest($"collections/{collectionId}/values/{key}");
            var response = await this.httpClient.DeleteAsync(request);
            this.CheckStatusCode(response, request);
        }

        private HttpRequest CreateRequest(string path, ValueApiModel content = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/{path}");
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (content != null)
            {
                request.SetContent(content);
            }

            string tenantId = this._httpContextAccessor.HttpContext.Request.GetTenant();
            request.Headers.Add(TENANT_HEADER, tenantId);

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
            return request;
        }

        private void CheckStatusCode(IHttpResponse response, IHttpRequest request)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            this.log.Info($"StorageAdapter returns {response.StatusCode} for request {request.Uri}", () => new
            {
                request.Uri,
                response.StatusCode,
                response.Content
            });

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException($"{response.Content}, request URL = {request.Uri}");

                case HttpStatusCode.Conflict:
                    throw new ConflictingResourceException($"{response.Content}, request URL = {request.Uri}");

                default:
                    throw new HttpRequestException($"Http request failed, status code = {response.StatusCode}, content = {response.Content}, request URL = {request.Uri}");
            }
        }

        private Http.HttpRequest PrepareRequest(string path, ValueApiModel content = null)
        {
            string tenantId = null;
            if (this._httpContextAccessor.HttpContext != null)
            {
                tenantId = this._httpContextAccessor.HttpContext.Request.GetTenant();
            }
            else
            {
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
