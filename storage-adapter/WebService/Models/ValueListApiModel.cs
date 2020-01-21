using System.Collections.Generic;
using System.Linq;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.StorageAdapter.WebService.Models
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
                { "$type", $"ValueList;1" },
                { "$uri", $"/v1/collections/{collectionId}/values" }
            };
        }
    }
}
