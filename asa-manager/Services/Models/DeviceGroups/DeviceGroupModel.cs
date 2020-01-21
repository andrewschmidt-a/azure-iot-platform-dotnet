// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups
{
    public class DeviceGroupModel
    {
        public DeviceGroupModel(string id, string eTag, DeviceGroupDataModel data)
        {
            this.Id = id;
            this.ETag = eTag;
            this.DisplayName = data.DisplayName;
            this.Conditions = data.Conditions;
        }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Conditions")]
        public IEnumerable<DeviceGroupConditionModel> Conditions { get; set; }

        [JsonProperty("ETag")]
        public string ETag { get; set; }
    }
}
