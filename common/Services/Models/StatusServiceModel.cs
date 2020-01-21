// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    public class StatusServiceModel
    {
        public StatusServiceModel(bool isHealthy, string message)
        {
            this.Status = new StatusResultServiceModel(isHealthy, message);
            this.Dependencies = new Dictionary<string, StatusResultServiceModel>();
            this.Properties = new Dictionary<string, string>();
        }

        [JsonProperty(PropertyName = "Status")]
        public StatusResultServiceModel Status { get; set; }

        [JsonProperty(PropertyName = "Properties")]
        public Dictionary<string, string> Properties { get; set; }

        [JsonProperty(PropertyName = "Dependencies")]
        public Dictionary<string, StatusResultServiceModel> Dependencies { get; set; }
    }
}
