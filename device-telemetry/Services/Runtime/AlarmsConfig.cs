// Copyright (c) Microsoft. All rights reserved.

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Runtime
{
    public class AlarmsConfig
    {
        public StorageConfig StorageConfig { get; set; }
        public int MaxDeleteRetries { get; set; }

        public AlarmsConfig(
            string cosmosDbDatabase,
            string cosmosDbCollection,
            int maxDeleteRetries)
        {
            this.StorageConfig = new StorageConfig(cosmosDbDatabase);
            this.MaxDeleteRetries = maxDeleteRetries;
        }
    }
}
