// Copyright (c) Microsoft. All rights reserved.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Models
{
    public class DeviceGroupApiModel
    {
        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Conditions")]
        public IEnumerable<DeviceGroupConditionModel> Conditions { get; set; }

        public DeviceGroupApiModel()
        {
        }
    }
}
