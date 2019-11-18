using System;

namespace Mmm.Platform.IoT.Common.Services.External.CosmosDb
{
    public interface IStorageClientConfig
    {
        Uri CosmosDbUri { get; }
        string CosmosDbKey { get; }
        int CosmosDbThroughput { get; set; }
    }
}