using Newtonsoft.Json;

namespace Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups
{
    public class DeviceModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
    }
}