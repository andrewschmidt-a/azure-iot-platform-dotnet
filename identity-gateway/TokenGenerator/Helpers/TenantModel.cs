using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace tenant_manager.Models
{
    public class TenantModel : TableEntity
    {
        public TenantModel()
        {
        }
        public TenantModel(string userId, string tenantGuid)
        {
            PartitionKey = userId;
            RowKey = tenantGuid;
        }
        public string Roles {  get; set; }

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