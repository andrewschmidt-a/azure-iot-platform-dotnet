// <copyright file="ValueListApiModel.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.StorageAdapter.WebService.Models
{
    public class ValueListApiModel
    {
        public ValueListApiModel(IEnumerable<ValueServiceModel> models, string collectionId)
        {
            this.Items = models.Select(m => new ValueApiModel(m));

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"ValueList;1" },
                { "$uri", $"/v1/collections/{collectionId}/values" },
            };
        }

        [JsonProperty("Items")]
        public IEnumerable<ValueApiModel> Items { get; private set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}