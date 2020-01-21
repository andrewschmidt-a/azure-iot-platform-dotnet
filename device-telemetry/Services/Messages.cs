// <copyright file="Messages.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Newtonsoft.Json.Linq;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services
{
    public class Messages : IMessages
    {
        private const string DataPropertyName = "data";
        private const string DataPrefix = DataPropertyName + ".";
        private const string SystemPrefix = "_";
        private const string DataSchemaType = DataPrefix + "schema";
        private const string DataPartitionId = "PartitionId";
        private const string TsiStorageTypeKey = "tsi";
        private const string TenantInfoKey = "tenant";
        private const string TelemetryCollectionKey = "telemetry-collection";
        private readonly ILogger logger;
        private readonly IStorageClient storageClient;
        private readonly ITimeSeriesClient timeSeriesClient;
        private readonly AppConfig config;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAppConfigurationClient appConfigurationClient;

        private readonly bool timeSeriesEnabled;
        private readonly DocumentClient documentClient;
        private readonly string databaseName;

        public Messages(
            AppConfig config,
            IStorageClient storageClient,
            ITimeSeriesClient timeSeriesClient,
            ILogger<Messages> logger,
            IHttpContextAccessor contextAccessor,
            IAppConfigurationClient appConfigurationClient)
        {
            this.storageClient = storageClient;
            this.timeSeriesClient = timeSeriesClient;
            this.timeSeriesEnabled = config.DeviceTelemetryService.Messages.TelemetryStorageType.Equals(
                TsiStorageTypeKey, StringComparison.OrdinalIgnoreCase);
            this.documentClient = storageClient.GetDocumentClient();
            this.databaseName = config.DeviceTelemetryService.Messages.Database;
            this.logger = logger;
            this.config = config;
            this.httpContextAccessor = contextAccessor;
            this.appConfigurationClient = appConfigurationClient;
        }

        private string CollectionId
        {
            get
            {
                return this.appConfigurationClient.GetValue(
                    $"{TenantInfoKey}:{httpContextAccessor.HttpContext.Request.GetTenant()}:{TelemetryCollectionKey}");
            }
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
                await this.GetListFromCosmosDbAsync(from, to, order, skip, limit, devices);
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

        private async Task<MessageList> GetListFromCosmosDbAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            int dataPrefixLen = DataPrefix.Length;

            var sql = QueryBuilder.GetDocumentsSql(
                "message",
                null,
                null,
                from,
                "_timeReceived",
                to,
                "_timeReceived",
                order,
                "_timeReceived",
                skip,
                limit,
                devices,
                "deviceId");

            logger.LogDebug("Created message query {sql}", sql);

            FeedOptions queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;

            List<Document> docs = await this.storageClient.QueryDocumentsAsync(
                this.databaseName,
                this.CollectionId,
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
                    if (!item.Key.StartsWith(SystemPrefix) && item.Key != "id" && item.Key != "deviceId")
                    {
                        string key = item.Key.ToString();
                        data.Add(key, item.Value);

                        // Telemetry types auto-discovery magic through union of all keys
                        properties.Add(key);
                    }
                }

                messages.Add(new Message(
                    doc.GetPropertyValue<string>("deviceId"),
                    doc.GetPropertyValue<long>("_timeReceived"),
                    data));
            }

            return new MessageList(messages, new List<string>(properties));
        }
    }
}