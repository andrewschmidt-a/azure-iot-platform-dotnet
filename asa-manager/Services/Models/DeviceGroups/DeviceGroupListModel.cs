using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups
{
    public class DeviceGroupListModel
    {
        [JsonProperty("Items")]
        public IEnumerable<DeviceGroupModel> Items { get; set; }
    }
}
