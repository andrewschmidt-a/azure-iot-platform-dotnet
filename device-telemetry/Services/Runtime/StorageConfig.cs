// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Runtime
{
    public class StorageConfig
    {
        public string CosmosDbDatabase { get; set; }

        public StorageConfig(
            string cosmosDbDatabase)
        {
            this.CosmosDbDatabase = cosmosDbDatabase;
            if (string.IsNullOrEmpty(this.CosmosDbDatabase))
            {
                throw new Exception("CosmosDb database name is empty in configuration");
            }
        }
    }
}
