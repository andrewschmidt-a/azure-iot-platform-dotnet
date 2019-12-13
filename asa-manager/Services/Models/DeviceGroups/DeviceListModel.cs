// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups
{
    public class DeviceListModel
    {
        [JsonProperty("Items")]
        public IEnumerable<DeviceModel> Items { get; set; }

        public string ContinuationToken { get; set; }
    }
}
