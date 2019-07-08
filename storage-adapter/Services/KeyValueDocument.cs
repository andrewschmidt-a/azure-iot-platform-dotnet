// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
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
            this.CollectionId = ((Dictionary<string, string>)((ClaimsPrincipal)Thread.CurrentPrincipal).Claims)["tenant"]+ "_" + collectionId;
            this.Key = key;
            this.Data = data;
        }
    }
}
