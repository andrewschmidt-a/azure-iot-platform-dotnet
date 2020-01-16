using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.DeviceTelemetry.Services.Models;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services
{
    public class Alarms : IAlarms
    {
        private readonly ILogger _logger;
        private readonly IStorageClient storageClient;
        private readonly AppConfig config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IAppConfigurationClient _appConfigurationClient;

        private readonly string databaseName;
        private readonly int maxDeleteRetryCount;

        // constants for storage keys
        private const string MESSAGE_RECEIVED_KEY = "device.msg.received";
        private const string RULE_ID_KEY = "rule.id";
        private const string DEVICE_ID_KEY = "device.id";
        private const string STATUS_KEY = "status";
        private const string ALARM_SCHEMA_KEY = "alarm";

        private const string ALARM_STATUS_OPEN = "open";
        private const string ALARM_STATUS_ACKNOWLEDGED = "acknowledged";

        private const string TENANT_INFO_KEY = "tenant";
        private const string TELEMETRY_COLLECTION_KEY = "telemetry-collection";

        private const int DOC_QUERY_LIMIT = 1000;


        private string collectionId
        {
            get
            {
                return this._appConfigurationClient.GetValue(
                    $"{TENANT_INFO_KEY}:{_httpContextAccessor.HttpContext.Request.GetTenant()}:{TELEMETRY_COLLECTION_KEY}");
            }
        }

        public Alarms(
            AppConfig config,
            IStorageClient storageClient,
            ILogger<Alarms> logger,
            IHttpContextAccessor contextAccessor,
            IAppConfigurationClient appConfigurationClient)
        {
            this.storageClient = storageClient;
            this.databaseName = config.DeviceTelemetryService.Alarms.Database;
            _logger = logger;
            this.maxDeleteRetryCount = config.DeviceTelemetryService.Alarms.MaxDeleteRetries;
            this.config = config;
            this._httpContextAccessor = contextAccessor;
            this._appConfigurationClient = appConfigurationClient;

        }

        public async Task<Alarm> GetAsync(string id)
        {
            Document doc = await this.GetDocumentByIdAsync(id);
            return new Alarm(doc);
        }

        public async Task<List<Alarm>> ListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            var sql = QueryBuilder.GetDocumentsSql(
                ALARM_SCHEMA_KEY,
                null, null,
                from, MESSAGE_RECEIVED_KEY,
                to, MESSAGE_RECEIVED_KEY,
                order, MESSAGE_RECEIVED_KEY,
                skip,
                limit,
                devices, DEVICE_ID_KEY);

            _logger.LogDebug("Created alarm query {sql}", sql);

            FeedOptions queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;

            List<Document> docs = await this.storageClient.QueryDocumentsAsync(
                this.databaseName,
                this.collectionId,
                queryOptions,
                sql,
                skip,
                limit);

            List<Alarm> alarms = new List<Alarm>();

            foreach (Document doc in docs)
            {
                alarms.Add(new Alarm(doc));
            }

            return alarms;
        }

        public async Task<List<Alarm>> ListByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            var sql = QueryBuilder.GetDocumentsSql(
                ALARM_SCHEMA_KEY,
                id, RULE_ID_KEY,
                from, MESSAGE_RECEIVED_KEY,
                to, MESSAGE_RECEIVED_KEY,
                order, MESSAGE_RECEIVED_KEY,
                skip,
                limit,
                devices, DEVICE_ID_KEY);

            _logger.LogDebug("Created alarm by rule query {sql}", sql);

            FeedOptions queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;

            List<Document> docs = await this.storageClient.QueryDocumentsAsync(
                this.databaseName,
                this.collectionId,
                queryOptions,
                sql,
                skip,
                limit);

            List<Alarm> alarms = new List<Alarm>();
            foreach (Document doc in docs)
            {
                alarms.Add(new Alarm(doc));
            }

            return alarms;
        }

        public async Task<int> GetCountByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string[] devices)
        {
            // build sql query to get open/acknowledged alarm count for rule
            string[] statusList = { ALARM_STATUS_OPEN, ALARM_STATUS_ACKNOWLEDGED };
            var sql = QueryBuilder.GetCountSql(
                ALARM_SCHEMA_KEY,
                id, RULE_ID_KEY,
                from, MESSAGE_RECEIVED_KEY,
                to, MESSAGE_RECEIVED_KEY,
                devices, DEVICE_ID_KEY,
                statusList, STATUS_KEY);

            FeedOptions queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;

            // request count of alarms for a rule id with given parameters
            var result = await this.storageClient.QueryCountAsync(
                this.databaseName,
                this.collectionId,
                queryOptions,
                sql);

            return result;
        }

        public async Task<Alarm> UpdateAsync(string id, string status)
        {
            InputValidator.Validate(id);
            InputValidator.Validate(status);

            Document document = await this.GetDocumentByIdAsync(id);
            document.SetPropertyValue(STATUS_KEY, status);

            document = await this.storageClient.UpsertDocumentAsync(
                this.databaseName,
                this.collectionId,
                document);

            return new Alarm(document);
        }

        private async Task<Document> GetDocumentByIdAsync(string id)
        {
            InputValidator.Validate(id);

            var query = new SqlQuerySpec(
                "SELECT * FROM c WHERE c.id=@id",
                new SqlParameterCollection(new SqlParameter[] {
                    new SqlParameter { Name = "@id", Value = id }
                })
            );
            // Retrieve the document using the DocumentClient.
            List<Document> documentList = await this.storageClient.QueryDocumentsAsync(
                this.databaseName,
                this.collectionId,
                null,
                query,
                0,
                DOC_QUERY_LIMIT);

            if (documentList.Count > 0)
            {
                return documentList[0];
            }

            return null;
        }

        public async Task Delete(List<string> ids)
        {
            foreach (var id in ids)
            {
                InputValidator.Validate(id);
            }

            Task[] taskList = new Task[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                taskList[i] = this.DeleteAsync(ids[i]);
            }

            try
            {
                await Task.WhenAll(taskList);
            }
            catch (AggregateException aggregateException)
            {
                Exception inner = aggregateException.InnerExceptions[0];
                _logger.LogError(inner, "Failed to delete alarm");
                throw inner;
            }
        }

        /**
         * Delete an individual alarm by id. If the delete fails for a DocumentClientException
         * other than not found, retry up to this.maxRetryCount
         */
        public async Task DeleteAsync(string id)
        {
            InputValidator.Validate(id);

            int retryCount = 0;
            while (retryCount < this.maxDeleteRetryCount)
            {
                try
                {
                    await this.storageClient.DeleteDocumentAsync(
                        this.databaseName,
                        this.collectionId,
                        id);
                    return;
                }
                catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return;
                }
                catch (Exception e)
                {
                    // only delay if there is a suggested retry (i.e. if the request is throttled)
                    TimeSpan retryTimeSpan = TimeSpan.Zero;
                    if (e.GetType() == typeof(DocumentClientException))
                    {
                        retryTimeSpan = ((DocumentClientException)e).RetryAfter;
                    }
                    retryCount++;

                    if (retryCount >= this.maxDeleteRetryCount)
                    {
                        _logger.LogError(e, "Failed to delete alarm {id}", id);
                        throw new ExternalDependencyException(e.Message);
                    }

                    _logger.LogWarning(e, "Exception on delete alarm {id}", id);
                    Thread.Sleep(retryTimeSpan);
                }
            }
        }
    }
}
