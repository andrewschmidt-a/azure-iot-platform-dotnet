using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace IdentityGateway.Services.Models
{
    public class UserTenantModel : TableEntity
    {
        public string Roles { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public UserTenantModel() { }

        public UserTenantModel(string userId, string tenantId)
        {
            this.PartitionKey = userId;
            this.RowKey = tenantId;
            this.Roles = "";
        }

        public UserTenantModel(string userId, string tenantId, string roles)
        {
            this.PartitionKey = userId;
            this.RowKey = tenantId;
            this.Roles = roles;
        }

        public UserTenantModel(UserTenantInput input)
        {
            this.PartitionKey = input.UserId;
            this.RowKey = input.Tenant;
            this.Roles = input.Roles;
            this.Name = input.Name;
            this.Type = input.Type;
        }

        public UserTenantModel(DynamicTableEntity tableEntity)
        {
            this.PartitionKey = tableEntity.PartitionKey;
            this.RowKey = tableEntity.RowKey;
            this.Roles = tableEntity.Properties["Roles"].StringValue;
            this.Name = tableEntity.Properties.Keys.Contains("Name") ? tableEntity.Properties["Name"].StringValue : this.PartitionKey;
            this.Type = tableEntity.Properties.Keys.Contains("Type") ? tableEntity.Properties["Type"].StringValue : "Member";
        }

        // Define aliases for the partition and row keys
        public string UserId
        {
            get
            {
                return this.PartitionKey;
            }
        }

        public string TenantId
        {
            get
            {
                return this.RowKey;
            }
        }

        public List<string> RoleList
        {
            get
            {
                try
                {
                    return this.Roles == null ? null : JsonConvert.DeserializeObject<List<string>>(this.Roles);
                }
                catch
                {
                    return new List<string>(); // cant Deserialize return Empty List
                }
            }
        }

        public static explicit operator UserTenantModel(DynamicTableEntity v) => new UserTenantModel(v);
    }
}