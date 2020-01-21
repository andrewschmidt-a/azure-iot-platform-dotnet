using System.Collections.Generic;
using Mmm.Platform.IoT.Config.Services.External;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Config.WebService.Models
{
    public class ConfigTypeListApiModel
    {
        public ConfigTypeListApiModel(ConfigTypeListServiceModel configTypeList)
        {
            this.ConfigTypes = configTypeList.ConfigTypes;

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;1" },
                { "$url", $"/v1/deviceproperties" },
            };
        }

        [JsonProperty("Items")]
        public string[] ConfigTypes { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}
