using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmIdListApiModel
    {
        public AlarmIdListApiModel()
        {
            this.Items = null;
        }

        public AlarmIdListApiModel(List<string> items)
        {
            this.Items = items;
        }

        [JsonProperty(PropertyName = "Items")]
        public List<string> Items { get; set; }
    }
}
