using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace IdentityGateway.Services.Models
{
    public class UserModel : TableEntity
    {
        public string TenantId;
        public string UserId;
        public string Roles {  get; set; }

        public UserModel(string userId, string tenantId)
        {
            this.UserId = userId;
            this.TenantId = tenantId;
            this.PartitionKey = this.UserId;
            this.RowKey = this.TenantId;
            this.Roles = "";
        }

        public UserModel(string userId, string tenantId, string roles)
        {
            this.UserId = userId;
            this.TenantId = tenantId;
            this.PartitionKey = this.TenantId;
            this.RowKey = this.UserId;
            this.Roles = roles;
        }

        public List<string> RoleList
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(this.Roles);
                }
                catch
                {
                    return new List<string>(); // cant Deserialize return Empty List
                }
            }
        }
    }
}