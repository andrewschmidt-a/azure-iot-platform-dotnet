using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    public class StatusResultServiceModel
    {
        [JsonConstructor]
        public StatusResultServiceModel(bool isHealthy, string message)
        {
            IsHealthy = isHealthy;
            Message = message;
        }

        [JsonProperty(PropertyName = "IsHealthy")]
        public bool IsHealthy { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }
    }
}