// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Http;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;
using HttpRequest = Microsoft.Azure.IoTSolutions.IotHubManager.Services.Http.HttpRequest;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.External
{
    public class StorageAdapterClient : IStorageAdapterClient
    {
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string TENANT_ID = "TenantID";
        private const string AZDS_ROUTE_KEY = "azds-route-as";
        
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StorageAdapterClient(IHttpClient httpClient, IServicesConfig config, ILogger logger, IHttpContextAccessor contextAccessor)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.StorageAdapterApiUrl;
            this._httpContextAccessor = contextAccessor;
        }

        public async Task<ValueApiModel> GetAsync(string collectionId, string key)
        {
            var request = this.CreateRequest($"collections/{collectionId}/values/{key}");
            var response = await this.httpClient.GetAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
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

            if (this._httpContextAccessor.HttpContext.Request.Headers.Count( p => p.Key == AZDS_ROUTE_KEY) > 0)
            {
                request.Headers.Add(AZDS_ROUTE_KEY, this._httpContextAccessor.HttpContext.Request.Headers.First(p => p.Key == AZDS_ROUTE_KEY).Value.First());
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
    }
}
