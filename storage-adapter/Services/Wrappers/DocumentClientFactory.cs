// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mmm.Platform.IoT.StorageAdapter.Services.Runtime;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Wrappers;

namespace Mmm.Platform.IoT.StorageAdapter.Services.Wrappers
{
    public class DocumentClientFactory : IFactory<IDocumentClient>
    {
        private IServicesConfig _config;
        private readonly ILogger _logger;
        private string connectionStringRegex = "^AccountEndpoint=(?<endpoint>.*);AccountKey=(?<key>.*);$";

        public DocumentClientFactory(IServicesConfig config, ILogger<DocumentClientFactory> logger)
        {
            this._config = config;
            _logger = logger;
        }

        public IDocumentClient Create()
        {
            try
            {
                var match = Regex.Match(this._config.DocumentDbConnString, this.connectionStringRegex);
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
