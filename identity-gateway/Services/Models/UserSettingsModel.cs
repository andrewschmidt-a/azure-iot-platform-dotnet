using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace IdentityGateway.Services.Models
{
    public class UserSettingsModel : TableEntity
    {
        public string UserId;
        public string SettingKey;
        public string Value;

        public UserSettingsModel(string userId, string settingKey, string value)
        {
            this.UserId = userId;
            this.PartitionKey = this.UserId;
            this.SettingKey = settingKey;
            this.RowKey = this.SettingKey;
            this.Value = value;
        }

        public UserSettingsModel(UserSettingsInput input)
        {
            this.UserId = input.userId;
            this.PartitionKey = this.UserId;
            this.SettingKey = input.settingKey;
            this.RowKey = this.SettingKey;
            this.Value = input.value;
        }
    }
}