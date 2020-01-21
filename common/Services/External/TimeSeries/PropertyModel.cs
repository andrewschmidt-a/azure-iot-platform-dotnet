using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.External.TimeSeries
{
    public class PropertyModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}