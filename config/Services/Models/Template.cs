using System.Collections.Generic;
using Mmm.Platform.IoT.Config.Services.External;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Config.Services.Models
{
    public class Template
    {
        [JsonProperty("Groups")]
        public IEnumerable<DeviceGroup> Groups { get; set; }

        [JsonProperty("Rules")]
        public IEnumerable<RuleApiModel> Rules { get; set; }

        [JsonProperty("Simulations")]
        public IEnumerable<SimulationApiModel> Simulations { get; set; }
    }
}
