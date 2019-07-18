// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;
using HttpRequest = Microsoft.Azure.IoTSolutions.UIConfig.Services.Http.HttpRequest;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class StorageAdapterClient : IStorageAdapterClient
    {
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string TENANT_ID = "TenantID";
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;


        public StorageAdapterClient(IHttpClient httpClient, IServicesConfig config, ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.StorageAdapterApiUrl;
            this._httpContextAccessor = httpContextAccessor;
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

        private HttpRequest CreateRequest(string path, ValueApiModel content = null, IHttpContextAccessor _httpContextAccessorLocal=null)
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
            request.Headers.Add(TENANT_HEADER, _httpContextAccessorLocal.HttpContext.Items[TENANT_ID].ToString());

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
