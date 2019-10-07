using System.Collections.Generic;
using Newtonsoft.Json;

namespace IdentityGateway.Services.Models
{
    public class UserTenantListModel
    {
        [JsonProperty("Method", Order=10)]
        public string batchMethod { get; set; }

        [JsonProperty("Models", Order=20)]
        public List<UserTenantModel> models { get; set; }

        public UserTenantListModel() { }

        public UserTenantListModel(List<UserTenantModel> models)
        {
            this.models = models;
        }

        public UserTenantListModel(string batchMethod, List<UserTenantModel> models)
        {
            this.batchMethod = batchMethod;
            this.models = models;
        }
    }
}