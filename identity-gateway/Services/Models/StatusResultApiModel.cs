using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityGateway.Services.Models
{
    public class StatusResultApiModel
    {
        [JsonProperty(PropertyName = "IsHealthy", Order = 10)]
        public bool IsHealthy { get; set; }

        [JsonProperty(PropertyName = "Message", Order = 20)]
        public string Message { get; set; }

        public StatusResultApiModel(StatusResultServiceModel servicemodel)
        {
            this.IsHealthy = servicemodel.IsHealthy;
            this.Message = servicemodel.Message;
        }
    }
}
