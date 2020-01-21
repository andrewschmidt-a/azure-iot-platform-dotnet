using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.External.StorageAdapter
{
    public class ValueListApiModel
    {
        public IList<ValueApiModel> Items { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}
