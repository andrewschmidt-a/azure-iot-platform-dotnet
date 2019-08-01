using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace tenant_manager.Models
{
    public class TenantModel : TableEntity
    {
        public string IotHubName { get; set; }
        public string IotHubConnectionString { get; set; }
        public string TelemetryCollectionName { get; set; }
        public bool IsIotHubDeployed { get; set; }
        public bool AreFunctionsUpdated { get; set; }

        public TenantModel (string id, string iotHubName, string telemetryCollectionName)
        {
            // Use the first character of the tenant id as the partion key as it is randomly distributed
            this.PartitionKey = id.Substring(0, 1);

            this.RowKey = id;
            this.IotHubName = iotHubName;
            this.IotHubConnectionString = "";
            this.TelemetryCollectionName = telemetryCollectionName;
            this.IsIotHubDeployed = false;
            this.AreFunctionsUpdated = false;
        }

        public TenantModel () { }
    }
}