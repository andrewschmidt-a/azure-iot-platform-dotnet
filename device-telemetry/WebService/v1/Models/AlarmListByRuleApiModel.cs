using System.Collections.Generic;
using Mmm.Platform.IoT.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmListByRuleApiModel : AlarmListApiModel
    {
        public AlarmListByRuleApiModel(List<Alarm> alarms)
            : base(alarms)
        {
        }

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public new Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", $"AlarmsByRule;1" },
            { "$uri", "/" + "v1/alarmsbyrule" }
        };
    }
}
