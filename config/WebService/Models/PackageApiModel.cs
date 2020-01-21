using System;
using Mmm.Platform.IoT.Config.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mmm.Platform.IoT.Config.WebService.Models
{
    public class PackageApiModel
    {
        [JsonProperty("Id")]
        public string Id;

        public PackageApiModel(PackageServiceModel model)
        {
            this.Id = model.Id;
            this.Name = model.Name;
            this.PackageType = model.PackageType;
            this.DateCreated = model.DateCreated;
            this.Content = model.Content;
            this.ConfigType = model.ConfigType;
        }

        public PackageApiModel(
                string content,
                string name,
                PackageType type,
                string configType)
        {
            this.Content = content;
            this.Name = name;
            this.PackageType = type;
            this.ConfigType = configType;
        }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("PackageType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType PackageType { get; set; }

        [JsonProperty("ConfigType")]
        public string ConfigType { get; set; }

        [JsonProperty(PropertyName = "DateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("Content")]
        public string Content { get; set; }

        public PackageServiceModel ToServiceModel()
        {
            return new PackageServiceModel()
            {
                Content = this.Content,
                Name = this.Name,
                PackageType = this.PackageType,
                ConfigType = this.ConfigType
            };
        }
    }
}