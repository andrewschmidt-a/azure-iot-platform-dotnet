// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.External.StorageAdapter
{
    public class StorageAdapterClient : ExternalServiceClient, IStorageAdapterClient
    {
        private readonly int timeout;

        public StorageAdapterClient(
            AppConfig config,
            IExternalRequestHelper requestHelper) :
            base(config.ExternalDependencies.StorageAdapterServiceUrl, requestHelper)
        {
            this.timeout = config.ExternalDependencies.StorageAdapterServiceTimeout;
        }

        public string RequestUrl(string path)
        {
            return $"{this.serviceUri}/{path}";
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
