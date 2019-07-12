using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace tenant_manager.Models
{
    public class TenantModel : TableEntity
    {
        public string IotHubConnectionString {  get; set; }

        public TenantModel (string id, string iotHubConnectionString)
        {
            this.PartitionKey = "test";
            this.RowKey = id;
            this.IotHubConnectionString = iotHubConnectionString;
        }
    }
}