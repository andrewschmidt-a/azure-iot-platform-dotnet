using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.TenantManager.Services.Models
{
    public class IdentityGatewayApiModel
    {
        public IdentityGatewayApiModel(string Roles)
        {
            this.Roles = Roles;
        }
        public IdentityGatewayApiModel() { }

        public string Roles { get; set; }

        public string UserId { get; set; }

        public string TenantId { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

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
