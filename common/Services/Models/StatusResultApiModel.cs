// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    public class StatusResultApiModel
    {
        [JsonProperty(PropertyName = "IsHealthy", Order = 10)]
        public bool IsHealthy { get; set; }

        [JsonProperty(PropertyName = "Message", Order = 20)]
        public string Message { get; set; }

        public StatusResultApiModel(StatusResultServiceModel serviceModel)
        {
            this.IsHealthy = serviceModel.IsHealthy;
            this.Message = serviceModel.Message;
        }
    }
}
