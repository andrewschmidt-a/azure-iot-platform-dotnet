// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Wrappers;
using System;
using System.Text.RegularExpressions;

namespace Mmm.Platform.IoT.StorageAdapter.Services.Wrappers
{
    public class DocumentClientFactory : IFactory<IDocumentClient>
    {
        private readonly AppConfig _appConfig;
        private readonly ILogger _logger;
        private string connectionStringRegex = "^AccountEndpoint=(?<endpoint>.*);AccountKey=(?<key>.*);$";

        public DocumentClientFactory(AppConfig appConfig, ILogger<DocumentClientFactory> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
        }

        public IDocumentClient Create()
        {
            try
            {
                var match = Regex.Match(_appConfig.Global.CosmosDb.DocumentDbConnectionString, this.connectionStringRegex);
                if (!match.Success)
                {
                    string message = "Invalid Connection String for CosmosDb";
                    throw new InvalidConfigurationException(message);
                }

                Uri docDbEndpoint = new Uri(match.Groups["endpoint"].Value);
                string docDbKey = match.Groups["key"].Value;
                return new DocumentClient(docDbEndpoint, docDbKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
