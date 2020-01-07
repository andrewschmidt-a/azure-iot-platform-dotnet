// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.External.StorageAdapter
{
    public class StorageAdapterClient : IStorageAdapterClient
    {
        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string TENANT_ID = "TenantID";
        private const string AZDS_ROUTE_KEY = "azds-route-as";

        private readonly string serviceUri;
        private readonly int timeout;
        private readonly IExternalRequestHelper _requestHelper;

        public StorageAdapterClient(AppConfig config, IExternalRequestHelper requestHelper)
        {
            this.serviceUri = config.ExternalDependencies.StorageAdapterWebServiceUrl;
            this.timeout = config.ExternalDependencies.StorageAdapterWebServiceTimeout;
            this._requestHelper = requestHelper;
        }

        public string RequestUrl(string path)
        {
            return $"{this.serviceUri}/{path}";
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                string url = this.RequestUrl("status/");
                var result = await this._requestHelper.ProcessRequestAsync<StatusServiceModel>(HttpMethod.Get, url);
                return result.Status;
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, $"Unable to get Storage Adapter Status: {e.Message}");
            }
        }

        public async Task<ValueApiModel> CreateAsync(string collectionId, string value)
        {
            string url = this.RequestUrl($"collections/{collectionId}/values");
            ValueApiModel data = new ValueApiModel
            {
                Data = value
            };
            return await this._requestHelper.ProcessRequestAsync(HttpMethod.Post, url, data);
        }

        public async Task<ValueApiModel> UpdateAsync(string collectionId, string key, string value, string etag)
        {
            string url = this.RequestUrl($"collections/{collectionId}/values/{key}");
            ValueApiModel data = new ValueApiModel
            {
                Data = value,
                ETag = etag
            };
            return await this._requestHelper.ProcessRequestAsync(HttpMethod.Put, url, data);
        }

        public async Task<ValueApiModel> GetAsync(string collectionId, string key)
        {
            string url = this.RequestUrl($"collections/{collectionId}/values/{key}");
            return await this._requestHelper.ProcessRequestAsync<ValueApiModel>(HttpMethod.Get, url);
        }

        public async Task<ValueListApiModel> GetAllAsync(string collectionId)
        {
            string url = this.RequestUrl($"collections/{collectionId}/values");
            return await this._requestHelper.ProcessRequestAsync<ValueListApiModel>(HttpMethod.Get, url);
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
            string url = this.RequestUrl($"collections/{collectionId}/values/{key}");
            await this._requestHelper.ProcessRequestAsync(HttpMethod.Delete, url);
        }
    }
}
