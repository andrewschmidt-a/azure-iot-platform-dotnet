using System;

namespace Mmm.Platform.IoT.Common.Services.External.CosmosDb
{
    public interface IStorageClientConfig
    {
        string CosmosDbConnectionString { get; set; }
        int CosmosDbThroughput { get; set; }
    }
}