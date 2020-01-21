using Newtonsoft.Json;

namespace Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups
{
    public class DeviceGroupConditionModel
    {
        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("Operator")]
        public string Operator { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}