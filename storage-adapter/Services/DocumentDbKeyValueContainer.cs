// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Wrappers;
using Mmm.Platform.IoT.StorageAdapter.Services.Helpers;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    public class DocumentDbKeyValueContainer : IKeyValueContainer, IDisposable
    {
        private const string COLLECTION_ID_KEY_FORMAT = "tenant:{0}:{1}-collection";

        private readonly IAppConfigurationClient _appConfigClient;
        private readonly AppConfig _appConfig;
        private readonly IExceptionChecker _exceptionChecker;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IStorageClient _client;

        private bool disposedValue;

        public virtual string DocumentDataType { get { return "pcs"; }}
        public virtual string DocumentDatabaseSuffix { get { return "storage"; }}

        public DocumentDbKeyValueContainer(
            IStorageClient client,
            IExceptionChecker exceptionChecker,
            AppConfig appConfig,
            IAppConfigurationClient appConfigHelper,
            ILogger<DocumentDbKeyValueContainer> logger,
            IHttpContextAccessor httpContextAcessor)
        {
            disposedValue = false;
            _client = client;
            _exceptionChecker = exceptionChecker;
            _appConfig = appConfig;
            _appConfigClient = appConfigHelper;
            _logger = logger;
            _httpContextAccessor = httpContextAcessor;
        }

        private string CollectionLink
        {
            get
            {
                return $"/dbs/{this.DocumentDbDatabaseId}/colls/{this.DocumentDbCollectionId}";
            }
        }

        public virtual string DocumentDbDatabaseId
        {
            get
            {
                return $"{this.DocumentDataType}-{this.DocumentDatabaseSuffix}";
            }
        }

        public virtual string DocumentDbCollectionId
        {
            get
            {
                string tenantId = this.TenantId;
                string key = String.Format(COLLECTION_ID_KEY_FORMAT, this.TenantId, this.DocumentDataType);
                try
                {
                    return this._appConfigClient.GetValue(key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to get the CollectionId from App Config. Key: {key}. TenantId: {tenantId}", key, this.TenantId);
                    throw;
                }
            }
        }

        public string TenantId
        {
            get
            {
                return this._httpContextAccessor.HttpContext.Request.GetTenant();
            }
        }

        public async Task<ValueServiceModel> GetAsync(string collectionId, string key)
        {
            try
            {
                var docId = DocumentIdHelper.GenerateId(collectionId, key);
                var response = await this._client.ReadDocumentAsync(this.DocumentDbDatabaseId, this.DocumentDbCollectionId, docId);
                return new ValueServiceModel(response);
            }
            catch (Exception ex)
            {
                if (!this._exceptionChecker.IsNotFoundException(ex)) throw;

                const string message = "The resource requested doesn't exist.";
                _logger.LogInformation(message + " {collection ID {collectionId}, key {key}", collectionId, key);
                throw new ResourceNotFoundException(message);
            }
        }

        public async Task<IEnumerable<ValueServiceModel>> GetAllAsync(string collectionId)
        {
            var query = await this._client.QueryAllDocumentsAsync(
                this.DocumentDbDatabaseId,
                this.DocumentDbCollectionId);
            return await Task
                .FromResult(query
                    .Select(doc => new ValueServiceModel(doc))
                    .Where(model => model.CollectionId == collectionId));
        }

        public async Task<ValueServiceModel> CreateAsync(string collectionId, string key, ValueServiceModel input)
        {
            try
            {
                var response = await this._client.CreateDocumentAsync(
                    this.DocumentDbDatabaseId,
                    this.DocumentDbCollectionId,
                    new KeyValueDocument(
                        collectionId,
                        key,
                        input.Data));
                return new ValueServiceModel(response);
            }
            catch (Exception ex)
            {
                if (!this._exceptionChecker.IsConflictException(ex)) throw;

                const string message = "There is already a value with the key specified.";
                _logger.LogInformation(message + " {collection ID {collectionId}, key {key}", collectionId, key);
                throw new ConflictingResourceException(message);
            }
        }

        public async Task<ValueServiceModel> UpsertAsync(string collectionId, string key, ValueServiceModel input)
        {
            try
            {
                var response = await this._client.UpsertDocumentAsync(
                    this.DocumentDbDatabaseId,
                    this.DocumentDbCollectionId,
                    new KeyValueDocument(
                        collectionId,
                        key,
                        input.Data));
                return new ValueServiceModel(response);
            }
            catch (Exception ex)
            {
                if (!this._exceptionChecker.IsPreconditionFailedException(ex)) throw;

                const string message = "ETag mismatch: the resource has been updated by another client.";
                _logger.LogInformation(message + " {collection ID {collectionId}, key {key}, ETag {eTag}", collectionId, key, input.ETag);
                throw new ConflictingResourceException(message);
            }
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
            try
            {
                string documentId = DocumentIdHelper.GenerateId(collectionId, key);
                await this._client.DeleteDocumentAsync(this.DocumentDbDatabaseId, this.DocumentDbCollectionId, documentId);
            }
            catch (Exception ex)
            {
                if (!this._exceptionChecker.IsNotFoundException(ex)) throw;
                _logger.LogDebug("Key {key} does not exist, nothing to do");
            }
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    (this._client as IDisposable)?.Dispose();
                }
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}
