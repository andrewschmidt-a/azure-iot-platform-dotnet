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
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.Common.Services.Wrappers;
using Mmm.Platform.IoT.StorageAdapter.Services.Helpers;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;
using Mmm.Platform.IoT.StorageAdapter.Services.Runtime;

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    public sealed class DocumentDbKeyValueContainer : IKeyValueContainer, IDisposable
    {
        private readonly IFactory<IDocumentClient> _clientFactory;
        private readonly IExceptionChecker _exceptionChecker;
        private readonly ILogger _logger;
        private readonly IServicesConfig _config;  // injected
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string documentDataType = "pcs";  // a datatype for this type of key value container. This could go into the constructor later if necessary

        private IDocumentClient client;
        private int docDbRUs;
        private RequestOptions docDbOptions;
        private bool disposedValue;


        public DocumentDbKeyValueContainer(
            IFactory<IDocumentClient> clientFactory,
            IExceptionChecker exceptionChecker,
            IServicesConfig config,
            ILogger<DocumentDbKeyValueContainer> logger,
            IHttpContextAccessor httpContextAcessor)
        {
            this.disposedValue = false;
            this._clientFactory = clientFactory;
            this._config = config;
            this._exceptionChecker = exceptionChecker;
            _logger = logger;
            this._httpContextAccessor = httpContextAcessor;
        }

        private string docDbDatabase
        {
            get
            {
                string docDbDatabase = this._config.DocumentDbDatabase;
                if (String.IsNullOrEmpty(docDbDatabase))
                {
                    _logger.LogInformation("A valid DocumentDb Database Id could not be retrieved for {docDataType}", this.documentDataType);
                    throw new Exception($"A valid DocumentDb Database Id could not be retrieved for {this.documentDataType}");
                }
                return docDbDatabase;
            }
        }

        private string docDbCollection
        {
            get
            {
                // TODO: Perhaps this should go into a Claims Helper? Much like our previous one?? ~ Andrew Schmidt
                try
                {
                    string tenant = this._httpContextAccessor.HttpContext.Request.GetTenant();
                    return this._config.DocumentDbCollection(tenant, this.documentDataType);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "A valid DocumentDb Collection Id was not included in the Claim");
                    throw;
                }
            }
        }

        private string collectionLink
        {
            get
            {
                return $"/dbs/{this.docDbDatabase}/colls/{this.docDbCollection}";
            }
        }

        public async Task<StatusResultServiceModel> PingAsync()
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
                var response = await this.client.ReadDocumentAsync($"{this.collectionLink}/docs/{docId}");
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

            var query = this.client.CreateDocumentQuery<KeyValueDocument>(this.collectionLink)
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
                    this.collectionLink,
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
                    this.collectionLink,
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
                await this.client.DeleteDocumentAsync($"{this.collectionLink}/docs/{DocumentIdHelper.GenerateId(collectionId, key)}");
            }
            catch (Exception ex)
            {
                if (!this._exceptionChecker.IsNotFoundException(ex)) throw;

                _logger.LogDebug("Key {key} does not exist, nothing to do");
            }
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
            this.docDbRUs = this._config.DocumentDbRUs;
            this.docDbOptions = this.GetDocDbOptions();
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                var uri = "/dbs/" + this.docDbDatabase;
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
                var uri = $"/dbs/{this.docDbDatabase}/colls/{this.docDbCollection}";
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
                _logger.LogInformation("Creating DocumentDb database {docDbDatabase}", docDbDatabase);
                var db = new Database { Id = this.docDbDatabase };
                await this.client.CreateDatabaseAsync(db);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    _logger.LogWarning("Another process already created the database {docDbDatabase}", docDbDatabase);
                }

                _logger.LogError(e, "Error while creating DocumentDb database {docDbDatabase}", docDbDatabase);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating DocumentDb database {docDbDatabase}", docDbDatabase);
                throw;
            }
        }

        private async Task CreateCollectionAsync()
        {
            try
            {
                _logger.LogInformation("Creating DocumentDb collection {docDbCollection}", docDbCollection);
                var coll = new DocumentCollection { Id = this.docDbCollection };

                var index = Index.Range(DataType.String, -1);
                var indexing = new IndexingPolicy(index) { IndexingMode = IndexingMode.Consistent };
                coll.IndexingPolicy = indexing;

                // Partitioning can be enabled in case the storage adapter is used to store 100k+ records
                //coll.PartitionKey = new PartitionKeyDefinition { Paths = new Collection<string> { "/CollectionId" } };

                var dbUri = "/dbs/" + this.docDbDatabase;
                await this.client.CreateDocumentCollectionAsync(dbUri, coll, this.docDbOptions);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    _logger.LogWarning("Another process already created the collection {docDbCollection}", docDbCollection);
                }

                _logger.LogError(e, "Error while creating DocumentDb collection {docDbCollection}", docDbCollection);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating DocumentDb collection {docDbCollection}", docDbDatabase);
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

        #region IDisposable Support

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

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}
