using Microsoft.Azure.Cosmos.Table;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Models
{
    public class TenantModel : TableEntity
    {
        public string IotHubName { get; set; }
        public string IotHubConnectionString { get; set; }
        public bool IsIotHubDeployed { get; set; }

        public TenantModel (string id, string iotHubName)
        {
            // Use the first character of the tenant id as the partion key as it is randomly distributed
            this.PartitionKey = id.Substring(0, 1);

            this.RowKey = id;
            this.IotHubName = iotHubName;
            this.IotHubConnectionString = "";
            this.IsIotHubDeployed = false;
        }

        public TenantModel () { }
    }
}