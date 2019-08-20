using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityGateway.Services.Models
{
    public class StatusResultServiceModel
    {
        [JsonProperty(PropertyName = "IsHealthy")]
        public bool IsHealthy { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }

        [JsonConstructor]
        public StatusResultServiceModel(bool isHealthy, string message)
        {
            IsHealthy = isHealthy;
            Message = message;
        }
    }
}
