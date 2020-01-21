using System;
using Azure.Data.AppConfiguration;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    public class AppConfigCacheValue
    {
        public AppConfigCacheValue(ConfigurationSetting value)
        {
            this.Value = value;
            this.ExpirationTime = DateTime.UtcNow.AddHours(1);
        }

        public AppConfigCacheValue(string key, string value)
            : this(new ConfigurationSetting(key, value))
        {
        }

        public ConfigurationSetting Value { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}