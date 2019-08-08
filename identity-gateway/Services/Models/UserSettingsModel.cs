using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace IdentityGateway.Services.Models
{
    public class UserSettingsModel : TableEntity
    {
        public string UserId;
        public string Value;

        public UserSettingsModel(string userId, string settingKey, string value)
        {
            this.UserId = userId;
            this.PartitionKey = this.UserId;
            this.RowKey = settingKey;
            this.Value = value;
        }
    }
}