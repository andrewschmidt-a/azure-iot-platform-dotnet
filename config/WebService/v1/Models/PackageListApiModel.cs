// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Mmm.Platform.IoT.Config.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Config.WebService.v1.Models
{
    public class PackageListApiModel
    {
        public PackageListApiModel(IEnumerable<PackageServiceModel> models)
        {
            this.Items = models.Select(m => new PackageApiModel(m));

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;1" },
                { "$url", $"/v1/deviceproperties" }
            };
        }

        public IEnumerable<PackageApiModel> Items { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}