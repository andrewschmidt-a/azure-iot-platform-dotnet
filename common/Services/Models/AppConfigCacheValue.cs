using Azure.Data.AppConfiguration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    class AppConfigCacheValue
    {
        public ConfigurationSetting Value { get; set; }
        public DateTime ExpirationTime { get; set; }

        public AppConfigCacheValue(ConfigurationSetting value)
        {
            this.Value = value;
            this.ExpirationTime = DateTime.UtcNow.AddHours(1);
        }

        public AppConfigCacheValue(string key, string value)
            : this(new ConfigurationSetting(key, value))
        {
        }
    }
}
