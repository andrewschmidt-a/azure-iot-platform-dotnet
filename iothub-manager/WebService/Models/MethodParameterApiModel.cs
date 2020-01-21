using System;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Models
{
    public class MethodParameterApiModel
    {
        public MethodParameterApiModel()
        {
        }

        public MethodParameterApiModel(MethodParameterServiceModel serviceModel)
        {
            if (serviceModel != null)
            {
                this.Name = serviceModel.Name;
                this.ResponseTimeout = serviceModel.ResponseTimeout;
                this.ConnectionTimeout = serviceModel.ConnectionTimeout;
                this.JsonPayload = serviceModel.JsonPayload;
            }
        }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "ResponseTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? ResponseTimeout { get; set; }

        [JsonProperty(PropertyName = "ConnectionTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? ConnectionTimeout { get; set; }

        [JsonProperty(PropertyName = "JsonPayload")]
        public string JsonPayload { get; set; }

        public MethodParameterServiceModel ToServiceModel()
        {
            return new MethodParameterServiceModel()
            {
                Name = this.Name,
                ResponseTimeout = this.ResponseTimeout,
                ConnectionTimeout = this.ConnectionTimeout,
                JsonPayload = this.JsonPayload
            };
        }
    }
}
