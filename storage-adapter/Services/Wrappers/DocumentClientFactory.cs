// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Wrappers
{
    public class DocumentClientFactory : IFactory<IDocumentClient>
    {
        private IServicesConfig _config;
        private ILogger _log;
        private string connectionStringRegex = "^AccountEndpoint=(?<endpoint>.*);AccountKey=(?<key>.*);$";

        public DocumentClientFactory(IServicesConfig config, ILogger logger)
        {
            this._config = config;
            this._log = logger;
        }

        public IDocumentClient Create()
        {
            try
            {
                if (String.IsNullOrEmpty(this._config.DocumentDbConnString))
                {
                    string message = "Configuration DocumentDbConnString is null or empty. The Connection String was not configured properly.";
                    throw new InvalidConfigurationException(message);
                }

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
                this._log.Error(ex.Message, () => { });
                throw;
            }
        }
    }
}
