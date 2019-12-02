// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Mmm.Platform.IoT.Config.Services.External;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Config.WebService.v1.Models
{
    public class ConfigTypeListApiModel
    {
        [JsonProperty("Items")]
        public string[] configTypes { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public ConfigTypeListApiModel(ConfigTypeListServiceModel configTypeList)
        {
            this.configTypes = configTypeList.ConfigTypes;

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/deviceproperties" }
            };
        }
    }
}
