// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Common.Services.External.StorageAdapter
{
    public interface IStorageAdapterClient
    {
        Task<ValueApiModel> GetAsync(string collectionId, string key);
        Task<ValueListApiModel> GetAllAsync(string collectionId);
        Task<ValueApiModel> CreateAsync(string collectionId, string value);
        Task<ValueApiModel> UpdateAsync(string collectionId, string key, string value, string etag);
        Task DeleteAsync(string collectionId, string key);
        Task<ValueApiModel> UpsertAsync(string collectionId, string key, string value, string etag);
    }
}
