// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Wrappers;

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
