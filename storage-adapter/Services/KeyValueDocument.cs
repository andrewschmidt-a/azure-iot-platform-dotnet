using System.Runtime.CompilerServices;
using Microsoft.Azure.Documents;
using Mmm.Platform.IoT.StorageAdapter.Services.Helpers;

[assembly: InternalsVisibleTo("Mmm.Platform.IoT.StorageAdapter.Services.Test")]

namespace Mmm.Platform.IoT.StorageAdapter.Services
{
    internal sealed class KeyValueDocument : Resource
    {
        public KeyValueDocument(string collectionId, string key, string data)
        {
            this.Id = DocumentIdHelper.GenerateId(collectionId, key);
            this.CollectionId = collectionId;
            this.Key = key;
            this.Data = data;
        }

        public string CollectionId { get; }

        public string Key { get; }

        public string Data { get; }
    }
}
