// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.Auth;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.CosmosDB;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services
{
    public interface IAlarms
    {
        Task<Alarm> GetAsync(string id);

        Task<List<Alarm>> ListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);

        Task<List<Alarm>> ListByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);

        Task<int> GetCountByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string[] devices);

        Task<Alarm> UpdateAsync(string id, string status);

        Task Delete(List<string> ids);

        Task DeleteAsync(string id);
    }

    public class Alarms : IAlarms
    {
        private readonly ILogger log;
        private readonly IStorageClient storageClient;
        private readonly IServicesConfig _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IAppConfigurationHelper _appConfigurationHelper;

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
                return this._appConfigurationHelper.GetValue(
                    $"{TENANT_INFO_KEY}:{_httpContextAccessor.HttpContext.Request.GetTenant()}:{TELEMETRY_COLLECTION_KEY}");
            }
        }

        public Alarms(
            IServicesConfig config,
            IStorageClient storageClient,
            ILogger logger,
            IHttpContextAccessor contextAccessor,
            IAppConfigurationHelper appConfigurationHelper)
        {
            this.storageClient = storageClient;
            this.databaseName = config.AlarmsConfig.StorageConfig.CosmosDbDatabase;
            this.log = logger;
            this.maxDeleteRetryCount = config.AlarmsConfig.MaxDeleteRetries;
            this._config = config;
            this._httpContextAccessor = contextAccessor;
            this._appConfigurationHelper = appConfigurationHelper;

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

            this.log.Debug("Created Alarm Query", () => new { sql });

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

            this.log.Debug("Created Alarm By Rule Query", () => new { sql });

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
            foreach(var id in ids)
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
                this.log.Error("Failed to delete alarm", () => new { inner });
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
                        retryTimeSpan = ((DocumentClientException) e).RetryAfter;
                    }
                    retryCount++;
                    
                    if (retryCount >= this.maxDeleteRetryCount)
                    {
                        this.log.Error("Failed to delete alarm", () => new { id, e });
                        throw new ExternalDependencyException(e);
                    }

                    this.log.Warn("Exception on delete alarm", () => new { id, e });
                    Thread.Sleep(retryTimeSpan);
                }
            }
        }
    }
}
