using System.Collections.Generic;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Models
{
    public class DevicePropertiesApiModel
    {
        public DevicePropertiesApiModel()
        {
        }

        public DevicePropertiesApiModel(List<string> model)
        {
            Items = model;
            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;1" },
                { "$url", $"/v1/deviceproperties" },
            };
        }

        [JsonProperty("Items")]
        public List<string> Items { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}