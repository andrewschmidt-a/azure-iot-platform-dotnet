using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mmm.Platform.IoT.Config.Services.Models
{
    public class PackageServiceModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType PackageType { get; set; }

        public string ConfigType { get; set; }

        public string Content { get; set; }

        public string DateCreated { get; set; }
    }
}