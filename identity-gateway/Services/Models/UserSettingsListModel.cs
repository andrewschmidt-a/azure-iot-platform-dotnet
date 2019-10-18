using System.Collections.Generic;
using Newtonsoft.Json;

namespace IdentityGateway.Services.Models
{
    public class UserSettingsListModel
    {
        [JsonProperty("Method", Order=10)]
        public string batchMethod { get; set; }

        [JsonProperty("Models", Order=20)]
        public List<UserSettingsModel> models { get; set; }

        public UserSettingsListModel() { }

        public UserSettingsListModel(List<UserSettingsModel> models)
        {
            this.models = models;
        }

        public UserSettingsListModel(string batchMethod, List<UserSettingsModel> models)
        {
            this.batchMethod = batchMethod;
            this.models = models;
        }
    }
}