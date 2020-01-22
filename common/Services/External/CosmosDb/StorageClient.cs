// <copyright file="StorageClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Mmm.Iot.Common.Services.Config;
using Mmm.Iot.Common.Services.Exceptions;
using Mmm.Iot.Common.Services.Models;

namespace Mmm.Iot.Common.Services.External.CosmosDb
{
    public class StorageClient : IStorageClient, IDisposable
    {
        private const string ConnectionStringValueRegex = @"^AccountEndpoint=(?<endpoint>.*);AccountKey=(?<key>.*);$";
        private const string StoragePartitionKey = "/deviceId";
        private readonly ILogger logger;
        private Uri storageUri;
        private string storagePrimaryKey;
        private int storageThroughput;
        private DocumentClient client;

        public StorageClient(AppConfig config, ILogger<StorageClient> logger)
        {
            this.SetValuesFromConfig(config);
            this.logger = logger;
            this.client = this.GetDocumentClient();
        }

        public async Task DeleteDatabaseAsync(string databaseName)
        {
            try
            {
                await this.client.DeleteDatabaseAsync($"/dbs/{databaseName}");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to delete database {databaseName}", databaseName);
                throw;
            }
        }

        public async Task CreateDatabaseIfNotExistsAsync(
            string databaseName)
        {
            Database database = new Database
            {
                Id = databaseName,
            };

            try
            {
                await this.client.CreateDatabaseIfNotExistsAsync(database);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to create database {databaseName}", databaseName);
                throw;
            }
        }

        public async Task DeleteCollectionAsync(
            string databaseName,
            string id)
        {
            string collectionLink = $"/dbs/{databaseName}/colls/{id}";
            try
            {
                await this.client.DeleteDocumentCollectionAsync(collectionLink);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to delete collection {id}.", databaseName);
                throw;
            }
        }

        public async Task<ResourceResponse<DocumentCollection>> CreateCollectionIfNotExistsAsync(
            string databaseName,
            string id)
        {
            DocumentCollection collectionInfo = new DocumentCollection();
            RangeIndex index = Index.Range(DataType.String, -1);
            collectionInfo.IndexingPolicy = new IndexingPolicy(
                new Index[] { index });
            collectionInfo.Id = id;
            collectionInfo.PartitionKey.Paths.Add(StoragePartitionKey);

            // Azure Cosmos DB collections can be reserved with
            // throughput specified in request units/second.
            RequestOptions requestOptions = new RequestOptions();
            requestOptions.OfferThroughput = this.storageThroughput;
            requestOptions.ConsistencyLevel = ConsistencyLevel.Strong;
            string dbUrl = "/dbs/" + databaseName;
            string colUrl = dbUrl + "/colls/" + id;
            ResourceResponse<DocumentCollection> response = null;

            try
            {
                await this.CreateDatabaseIfNotExistsAsync(databaseName);
            }
            catch (Exception e)
            {
                throw new Exception($"While attempting to create collection {id}, an error occured while attepmting to create its database {databaseName}.", e);
            }

            try
            {
                response = await this.client.CreateDocumentCollectionIfNotExistsAsync(
                    dbUrl,
                    collectionInfo,
                    requestOptions);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error creating collection with ID {id}, database URL {databaseUrl}, and collection info {collectionInfo}", id, dbUrl, collectionInfo);
                throw;
            }

            return response;
        }

        public async Task<Document> ReadDocumentAsync(
            string databaseName,
            string colId,
            string docId)
        {
            string docUrl = string.Format(
                "/dbs/{0}/colls/{1}/docs/{2}",
                databaseName,
                colId,
                docId);

            await this.CreateCollectionIfNotExistsAsync(databaseName, colId);
            try
            {
                return await this.client.ReadDocumentAsync(
                    docUrl,
                    new RequestOptions());
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error reading document in collection with collection ID {collectionId}", colId);
                throw;
            }
        }

        public async Task<Document> CreateDocumentAsync(
            string databaseName,
            string colId,
            object document)
        {
            string colUrl = string.Format("/dbs/{0}/colls/{1}", databaseName, colId);

            await this.CreateCollectionIfNotExistsAsync(databaseName, colId);
            try
            {
                return await this.client.CreateDocumentAsync(colUrl, document, new RequestOptions(), false);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error upserting document into collection with collection ID {collectionId}", colId);
                throw;
            }
        }

        public async Task<Document> DeleteDocumentAsync(
            string databaseName,
            string colId,
            string docId)
        {
            string docUrl = string.Format(
                "/dbs/{0}/colls/{1}/docs/{2}",
                databaseName,
                colId,
                docId);

            await this.CreateCollectionIfNotExistsAsync(databaseName, colId);
            try
            {
                return await this.client.DeleteDocumentAsync(
                    docUrl,
                    new RequestOptions());
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error deleting document in collection with collection ID {collectionId}", colId);
                throw;
            }
        }

        public DocumentClient GetDocumentClient()
        {
            if (this.client == null)
            {
                try
                {
                    this.client = new DocumentClient(
                        this.storageUri,
                        this.storagePrimaryKey,
                        ConnectionPolicy.Default,
                        ConsistencyLevel.Session);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Could not connect to storage at URI {storageUri}; check connection string", storageUri);
                    throw new InvalidConfigurationException(
                        "Could not connect to DocumentClient, " +
                        "check connection string");
                }

                if (this.client == null)
                {
                    logger.LogError("Could not connect to storage at URI {uri}", storageUri);
                    throw new InvalidConfigurationException(
                        "Could not connect to DocumentClient");
                }
            }

            return this.client;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
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
                logger.LogInformation(e, result.Message);
            }

            return result;
        }

        public async Task<List<Document>> QueryAllDocumentsAsync(
            string databaseName,
            string colId)
        {
            string collectionLink = string.Format(
                "/dbs/{0}/colls/{1}",
                databaseName,
                colId);

            await this.CreateCollectionIfNotExistsAsync(databaseName, colId);
            try
            {
                return this.client.CreateDocumentQuery(collectionLink).ToList<Document>();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to query collection {colId} for All documents", colId);
                throw;
            }
        }

        public async Task<List<Document>> QueryDocumentsAsync(
            string databaseName,
            string colId,
            FeedOptions queryOptions,
            SqlQuerySpec querySpec,
            int skip,
            int limit)
        {
            if (queryOptions == null)
            {
                queryOptions = new FeedOptions();
                queryOptions.EnableCrossPartitionQuery = true;
                queryOptions.EnableScanInQuery = true;
            }

            await this.CreateCollectionIfNotExistsAsync(databaseName, colId);

            List<Document> docs = new List<Document>();
            string collectionLink = string.Format(
                "/dbs/{0}/colls/{1}",
                databaseName,
                colId);

            var queryResults = this.client.CreateDocumentQuery<Document>(
                    collectionLink,
                    querySpec,
                    queryOptions)
                .AsEnumerable()
                .Skip(skip)
                .Take(limit);

            foreach (Document doc in queryResults)
            {
                docs.Add(doc);
            }

            logger.LogInformation("Query results count: {count}", docs.Count);

            return docs;
        }

        public async Task<int> QueryCountAsync(
            string databaseName,
            string colId,
            FeedOptions queryOptions,
            SqlQuerySpec querySpec)
        {
            if (queryOptions == null)
            {
                queryOptions = new FeedOptions();
                queryOptions.EnableCrossPartitionQuery = true;
                queryOptions.EnableScanInQuery = true;
            }

            string collectionLink = string.Format(
                "/dbs/{0}/colls/{1}",
                databaseName,
                colId);

            await this.CreateCollectionIfNotExistsAsync(databaseName, colId);
            var resultList = this.client.CreateDocumentQuery(
                collectionLink,
                querySpec,
                queryOptions).ToArray();

            if (resultList.Length > 0)
            {
                return (int)resultList[0];
            }

            logger.LogInformation("No results found for count query '{querySpec}' on collection with ID {collectionId} and database name {databaseName}", querySpec, colId, databaseName);

            return 0;
        }

        public async Task<Document> UpsertDocumentAsync(
            string databaseName,
            string colId,
            object document)
        {
            string colUrl = string.Format("/dbs/{0}/colls/{1}", databaseName, colId);

            await this.CreateCollectionIfNotExistsAsync(databaseName, colId);
            try
            {
                return await this.client.UpsertDocumentAsync(
                    colUrl,
                    document,
                    new RequestOptions(),
                    false);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error upserting document into collection with collection ID {collectionId}", colId);
                throw;
            }
        }

        public void Dispose()
        {
            if (this.client != null)
            {
                this.client.Dispose();
            }
        }

        private void SetValuesFromConfig(AppConfig config)
        {
            if (string.IsNullOrEmpty(config.Global.CosmosDb.DocumentDbConnectionString))
            {
                throw new ArgumentNullException("The CosmosDbConnectionString in the IStorageClientConfig was null or empty. The StorageClient cannot be created with an empty connection string.");
            }

            try
            {
                Match match = Regex.Match(config.Global.CosmosDb.DocumentDbConnectionString, ConnectionStringValueRegex);

                // Get the storage uri from the regular expression match
                Uri storageUriEndpoint;
                Uri.TryCreate(match.Groups["endpoint"].Value, UriKind.RelativeOrAbsolute, out storageUriEndpoint);
                this.storageUri = storageUriEndpoint;
                if (string.IsNullOrEmpty(this.storageUri.ToString()))
                {
                    throw new Exception("The StorageUri dissected from the connection string was null. The connection string may be null or not formatted correctly.");
                }

                // Get the PrimaryKey from the connection string
                this.storagePrimaryKey = match.Groups["key"]?.Value;
                if (string.IsNullOrEmpty(this.storagePrimaryKey))
                {
                    throw new Exception("The StoragePrimaryKey dissected from the connection string was null. The connection string may be null or not formatted correctly.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create required StorageClient fields using the connection string from the IStorageClientConfig instance.", e);
            }

            // handling exceptions is not necessary here - the value can be left null if not configured.
            this.storageThroughput = config.Global.CosmosDb.Rus;
        }
    }
}