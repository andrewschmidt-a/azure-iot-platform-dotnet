using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.External.CosmosDb
{
    public interface IStorageClient : IStatusOperation
    {
        DocumentClient GetDocumentClient();

        Task DeleteDatabaseAsync(
            string databaseName);

        Task CreateDatabaseIfNotExistsAsync(
            string databaseName);

        Task DeleteCollectionAsync(
            string databaseName,
            string id
        );

        Task<ResourceResponse<DocumentCollection>> CreateCollectionIfNotExistsAsync(
            string databaseName,
            string id);
        
        Task<Document> CreateDocumentAsync(
            string databaseName,
            string colId,
            object docId);
        
        Task<Document> ReadDocumentAsync(
            string databaseName,
            string colId,
            string docId);

        Task<Document> UpsertDocumentAsync(
            string databaseName,
            string colId,
            object document);

        Task<Document> DeleteDocumentAsync(
            string databaseName,
            string colId,
            string docId);

        Task<List<Document>> QueryAllDocumentsAsync(
            string databaseName,
            string colId);

        Task<List<Document>> QueryDocumentsAsync(
            string databaseName,
            string colId,
            FeedOptions queryOptions,
            SqlQuerySpec querySpec,
            int skip,
            int limit);

        Task<int> QueryCountAsync(
            string databaseName,
            string colId,
            FeedOptions queryOptions,
            SqlQuerySpec querySpec);
    }
}