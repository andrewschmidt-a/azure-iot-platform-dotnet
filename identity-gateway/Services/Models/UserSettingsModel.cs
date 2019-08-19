using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace IdentityGateway.Services.Models
{
    public class UserSettingsModel : TableEntity
    {
        public string Value { get; set; }

        public UserSettingsModel() { }

        public UserSettingsModel(string userId, string settingKey, string value)
        {
            this.PartitionKey = userId;
            this.RowKey = settingKey;
            this.Value = value;
        }

        public UserSettingsModel(UserSettingsInput input)
        {
            this.PartitionKey = input.userId;
            this.RowKey = input.settingKey;
            this.Value = input.value;
        }

        // Define aliases for the partition and row keys
        public string UserId
        {
            get
            {
                return this.PartitionKey;
            }
        }

        public string SettingKey
        {
            get
            {
                return this.RowKey;
            }
        }
    }
}