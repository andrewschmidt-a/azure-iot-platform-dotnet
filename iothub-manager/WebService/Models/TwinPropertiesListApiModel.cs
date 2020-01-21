using System.Collections.Generic;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Models
{
    public class TwinPropertiesListApiModel
    {
        public TwinPropertiesListApiModel()
        {
        }

        public TwinPropertiesListApiModel(TwinServiceListModel twins)
        {
            this.Items = new List<TwinPropertiesApiModel>();
            this.ContinuationToken = twins.ContinuationToken;
            foreach (var t in twins.Items)
            {
                this.Items.Add(new TwinPropertiesApiModel(
                    t.DesiredProperties,
                    t.ReportedProperties,
                    t.DeviceId,
                    t.ModuleId));
            }
        }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", "DeviceList;1" },
            { "$uri", "/" + "v1/devices" },
        };

        [JsonProperty(PropertyName = "ContinuationToken")]
        public string ContinuationToken { get; set; }

        [JsonProperty(PropertyName = "Items")]
        public List<TwinPropertiesApiModel> Items { get; set; }
    }
}
