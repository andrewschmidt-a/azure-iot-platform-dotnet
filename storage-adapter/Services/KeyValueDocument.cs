﻿// Copyright (c) Microsoft. All rights reserved.

using System.Runtime.CompilerServices;
using Microsoft.Azure.Documents;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;

[assembly: InternalsVisibleTo("Services.Test")]

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services
{
    internal sealed class KeyValueDocument : Resource
    {
        public string CollectionId { get; }
        public string Key { get; }
        public string Data { get; }

        public KeyValueDocument(string collectionId, string key, string data)
        {
            this.Id = DocumentIdHelper.GenerateId(collectionId, key);
            // TODO: Perhaps this should go into a Claims Helper? Much like our previous one?? ~ Andrew Schmidt
            this.CollectionId = collectionId;
            this.Key = key;
            this.Data = data;
        }
    }
}
