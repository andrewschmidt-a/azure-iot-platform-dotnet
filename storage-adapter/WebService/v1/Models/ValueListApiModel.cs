// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;
using Mmm.Platform.IoT.Common.WebService.v1;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.StorageAdapter.WebService.v1.Models
{
    public class ValueListApiModel
    {
        [JsonProperty("Items")]
        public readonly IEnumerable<ValueApiModel> Items;

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata;

        public ValueListApiModel(IEnumerable<ValueServiceModel> models, string collectionId)
        {
            this.Items = models.Select(m => new ValueApiModel(m));

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"ValueList;{Version.NUMBER}" },
                { "$uri", $"/{Version.PATH}/collections/{collectionId}/values" }
            };
        }
    }
}
