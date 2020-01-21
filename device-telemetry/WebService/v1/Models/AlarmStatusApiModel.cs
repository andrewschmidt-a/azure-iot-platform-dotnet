// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmStatusApiModel
    {
        public AlarmStatusApiModel()
        {
            this.Status = null;
        }

        public AlarmStatusApiModel(string status)
        {
            this.Status = status;
        }

        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }
    }
}
