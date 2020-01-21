// <copyright file="IKeyValueContainer.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    /// <summary>
    /// Common interface for underlying key-value storage services, such as Cosmos DB, Azure Storage Table and so on
    /// </summary>
    public interface IKeyValueContainer
    {
        string DocumentDataType { get; }

        string DocumentDatabaseSuffix { get; }

        string TenantId { get; }

        string DocumentDbDatabaseId { get; }

        string DocumentDbCollectionId { get; }

        Task<ValueServiceModel> GetAsync(string collectionId, string key);

        Task<IEnumerable<ValueServiceModel>> GetAllAsync(string collectionId);

        Task<ValueServiceModel> CreateAsync(string collectionId, string key, ValueServiceModel input);

        Task<ValueServiceModel> UpsertAsync(string collectionId, string key, ValueServiceModel input);

        Task DeleteAsync(string collectionId, string key);
    }
}