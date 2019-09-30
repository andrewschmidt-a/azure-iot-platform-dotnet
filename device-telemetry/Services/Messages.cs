// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.Auth;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.CosmosDB;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services
{
    public interface IMessages
    {
        Task<MessageList> ListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);
    }

    public class Messages : IMessages
    {
        private const string DATA_PROPERTY_NAME = "data";
        private const string DATA_PREFIX = DATA_PROPERTY_NAME + ".";
        private const string SYSTEM_PREFIX = "_";
        private const string DATA_SCHEMA_TYPE = DATA_PREFIX + "schema";
        private const string DATA_PARTITION_ID = "PartitionId";
        private const string TSI_STORAGE_TYPE_KEY = "tsi";
        private const string TENANT_INFO_KEY = "tenant";
        private const string TELEMETRY_COLLECTION_KEY = "telemetry-collection";

        private readonly ILogger log;
        private readonly IStorageClient storageClient;
        private readonly ITimeSeriesClient timeSeriesClient;
        private readonly IServicesConfig _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IAppConfigurationHelper _appConfigurationHelper;

        private readonly bool timeSeriesEnabled;
        private readonly DocumentClient documentClient;
        private readonly string databaseName;

        private string collectionId
        {
            get
            {
                return this._appConfigurationHelper.GetValue(
                    $"{TENANT_INFO_KEY}:{_httpContextAccessor.HttpContext.Request.GetTenant()}:{TELEMETRY_COLLECTION_KEY}");
            }
        }

        public Messages(
            IServicesConfig config,
            IStorageClient storageClient,
            ITimeSeriesClient timeSeriesClient,
            ILogger logger,
            IHttpContextAccessor contextAccessor,
            IAppConfigurationHelper appConfigurationHelper)
        {
            this.storageClient = storageClient;
            this.timeSeriesClient = timeSeriesClient;
            this.timeSeriesEnabled = config.StorageType.Equals(
                TSI_STORAGE_TYPE_KEY, StringComparison.OrdinalIgnoreCase);
            this.documentClient = storageClient.GetDocumentClient();
            this.databaseName = config.MessagesConfig.CosmosDbDatabase;
            this.log = logger;
            this._config = config;
            this._httpContextAccessor = contextAccessor;
            this._appConfigurationHelper = appConfigurationHelper;
        }

        public async Task<MessageList> ListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            InputValidator.Validate(order);
            foreach (var device in devices)
            {
                InputValidator.Validate(device);
            }

            return this.timeSeriesEnabled ? 
                await this.GetListFromTimeSeriesAsync(from, to, order, skip, limit, devices) : 
                this.GetListFromCosmosDb(from, to, order, skip, limit, devices);
        }

        private async Task<MessageList> GetListFromTimeSeriesAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            return await this.timeSeriesClient.QueryEventsAsync(from, to, order, skip, limit, devices);
        }

        private MessageList GetListFromCosmosDb(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            int dataPrefixLen = DATA_PREFIX.Length;

            var sql = QueryBuilder.GetDocumentsSql(
                "message",
                null, null,
                from, "_timeReceived",
                to, "_timeReceived",
                order, "_timeReceived",
                skip,
                limit,
                devices, "_deviceId");

            this.log.Debug("Created Message Query", () => new { sql });

            FeedOptions queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;

            List<Document> docs = this.storageClient.QueryDocuments(
                this.databaseName,
                this.collectionId,
                queryOptions,
                sql,
                skip,
                limit);

            // Messages to return
            List<Message> messages = new List<Message>();

            // Auto discovered telemetry types
            HashSet<string> properties = new HashSet<string>();

            foreach (Document doc in docs)
            {
                // Document fields to expose
                JObject data = new JObject();

                // Extract all the telemetry data and types
                var jsonDoc = JObject.Parse(doc.ToString());

                foreach (var item in jsonDoc)
                {
                    // Ignore fields that werent sent by device (system fields)"
                    if (!item.Key.StartsWith(SYSTEM_PREFIX) && item.Key != "id")
                    {
                        string key = item.Key.ToString();
                        data.Add(key, item.Value);

                        // Telemetry types auto-discovery magic through union of all keys
                        properties.Add(key);
                    }
                }
                messages.Add(new Message(
                    doc.GetPropertyValue<string>("_deviceId"),
                    doc.GetPropertyValue<long>("_timeReceived"),
                    data));
            }

            return new MessageList(messages, new List<string>(properties));
        }
    }
}
