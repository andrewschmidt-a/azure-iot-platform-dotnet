using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.External.StorageAdapter
{
    public class ValueListApiModel
    {
        public IList<ValueApiModel> Items;

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata;
    }
}
