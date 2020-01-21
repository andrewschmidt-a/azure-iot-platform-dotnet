// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.Common.Services.Wrappers;
using Mmm.Platform.IoT.StorageAdapter.Services.Helpers;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;
using Index = Microsoft.Azure.Documents.Index;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    public class DocumentDbKeyValueContainer : IKeyValueContainer, IDisposable
    {
        private const string COLLECTION_ID_KEY_FORMAT = "tenant:{0}:{1}-collection";
        private readonly IAppConfigurationHelper _appConfigHelper;
        private readonly AppConfig _appConfig;
        private readonly IFactory<IDocumentClient> _clientFactory;
        private readonly IExceptionChecker _exceptionChecker;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IDocumentClient client;
        private int docDbRUs;
        private RequestOptions docDbOptions;
        private bool disposedValue;

        public DocumentDbKeyValueContainer(
            IFactory<IDocumentClient> clientFactory,
            IExceptionChecker exceptionChecker,
            AppConfig appConfig,
            IAppConfigurationHelper appConfigHelper,
            ILogger<DocumentDbKeyValueContainer> logger,
            IHttpContextAccessor httpContextAcessor)
        {
            disposedValue = false;
            _clientFactory = clientFactory;
            _exceptionChecker = exceptionChecker;
            _appConfig = appConfig;
            _appConfigHelper = appConfigHelper;
            _logger = logger;
            _httpContextAccessor = httpContextAcessor;
        }

        public virtual string DocumentDataType { get { return "pcs"; } }
        public virtual string DocumentDatabaseSuffix { get { return "storage"; } }

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
                string key = string.Format(COLLECTION_ID_KEY_FORMAT, this.TenantId, this.DocumentDataType);
                try
                {
                    return this._appConfigHelper.GetValue(key);
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

        private string CollectionLink
        {
            get
            {
                return $"/dbs/{this.DocumentDbDatabaseId}/colls/{this.DocumentDbCollectionId}";
            }
        }
        public async Task<StatusResultServiceModel> StatusAsync()
        {
            this.SetClientOptions();
            var result = new StatusResultServiceModel(false, "Storage check failed");

            try
            {
                DatabaseAccount response = null;
                if (this.client != null)
                {
                    // make generic call to see if storage client can be reached
                    response = await this.client.GetDatabaseAccountAsync();
                }

                if (response != null)
                {
                    result.IsHealthy = true;
                    result.Message = "Alive and Well!";
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, result.Message);
            }

            return result;
        }

        public async Task<ValueServiceModel> GetAsync(string collectionId, string key)
        {
            await this.SetupStorageAsync();

            try
            {
                var docId = DocumentIdHelper.GenerateId(collectionId, key);
                var response = await this.client.ReadDocumentAsync($"{this.CollectionLink}/docs/{docId}");
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
            await this.SetupStorageAsync();

            var query = this.client.CreateDocumentQuery<KeyValueDocument>(this.CollectionLink)
                .Where(doc => doc.CollectionId.ToLower() == collectionId.ToLower())
                .ToList();
            return await Task.FromResult(query.Select(doc => new ValueServiceModel(doc)));
        }

        public async Task<ValueServiceModel> CreateAsync(string collectionId, string key, ValueServiceModel input)
        {
            await this.SetupStorageAsync();

            try
            {
                var response = await this.client.CreateDocumentAsync(
                    this.CollectionLink,
                    new KeyValueDocument(collectionId, key, input.Data));
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
            await this.SetupStorageAsync();

            try
            {
                var response = await this.client.UpsertDocumentAsync(
                    this.CollectionLink,
                    new KeyValueDocument(collectionId, key, input.Data),
                    IfMatch(input.ETag));
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
            await this.SetupStorageAsync();

            try
            {
                await this.client.DeleteDocumentAsync($"{this.CollectionLink}/docs/{DocumentIdHelper.GenerateId(collectionId, key)}");
            }
            catch (Exception ex)
            {
                if (!this._exceptionChecker.IsNotFoundException(ex)) throw;

                _logger.LogDebug("Key {key} does not exist, nothing to do");
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private RequestOptions GetDocDbOptions()
        {
            return new RequestOptions
            {
                OfferThroughput = this.docDbRUs,
                ConsistencyLevel = ConsistencyLevel.Strong
            };
        }

        private async Task SetupStorageAsync()
        {
            this.SetClientOptions();
            await this.CreateDatabaseIfNotExistsAsync();
            await this.CreateCollectionIfNotExistsAsync();
        }

        private void SetClientOptions()
        {
            this.client = this._clientFactory.Create();
            this.docDbRUs = _appConfig.StorageAdapterService.DocumentDbRus;
            this.docDbOptions = this.GetDocDbOptions();
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                var uri = "/dbs/" + this.DocumentDbDatabaseId;
                await this.client.ReadDatabaseAsync(uri, this.docDbOptions);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger.LogError(e, "Error while getting DocumentDb database");
                }

                await this.CreateDatabaseAsync();
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                var uri = $"/dbs/{this.DocumentDbDatabaseId}/colls/{this.DocumentDbCollectionId}";
                await this.client.ReadDocumentCollectionAsync(uri, this.docDbOptions);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger.LogError(e, "Error while getting DocumentDb collection");
                }

                await this.CreateCollectionAsync();
            }
        }

        private async Task CreateDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Creating DocumentDb database {DocumentDbDatabaseId}", DocumentDbDatabaseId);
                var db = new Database { Id = this.DocumentDbDatabaseId };
                await this.client.CreateDatabaseAsync(db);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    _logger.LogWarning("Another process already created the database {DocumentDbDatabaseId}", DocumentDbDatabaseId);
                }

                _logger.LogError(e, "Error while creating DocumentDb database {DocumentDbDatabaseId}", DocumentDbDatabaseId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating DocumentDb database {DocumentDbDatabaseId}", DocumentDbDatabaseId);
                throw;
            }
        }

        private async Task CreateCollectionAsync()
        {
            try
            {
                _logger.LogInformation("Creating DocumentDb collection {DocumentDbCollectionId}", DocumentDbCollectionId);
                var coll = new DocumentCollection { Id = this.DocumentDbCollectionId };

                var index = Index.Range(DataType.String, -1);
                var indexing = new IndexingPolicy(index) { IndexingMode = IndexingMode.Consistent };
                coll.IndexingPolicy = indexing;

                // Partitioning can be enabled in case the storage adapter is used to store 100k+ records
                // coll.PartitionKey = new PartitionKeyDefinition { Paths = new Collection<string> { "/CollectionId" } };

                var dbUri = "/dbs/" + this.DocumentDbDatabaseId;
                await this.client.CreateDocumentCollectionAsync(dbUri, coll, this.docDbOptions);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    _logger.LogWarning("Another process already created the collection {DocumentDbCollectionId}", DocumentDbCollectionId);
                }

                _logger.LogError(e, "Error while creating DocumentDb collection {DocumentDbCollectionId}", DocumentDbCollectionId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating DocumentDb collection {DocumentDbCollectionId}", DocumentDbDatabaseId);
                throw;
            }
        }

        private static RequestOptions IfMatch(string etag)
        {
            if (etag == "*")
            {
                // Match all
                return null;
            }
            return new RequestOptions
            {
                AccessCondition = new AccessCondition
                {
                    Condition = etag,
                    Type = AccessConditionType.IfMatch
                }
            };
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    (this.client as IDisposable)?.Dispose();
                }
                this.disposedValue = true;
            }
        }
    }
}
