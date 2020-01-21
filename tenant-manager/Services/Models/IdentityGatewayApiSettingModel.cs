namespace Mmm.Platform.IoT.TenantManager.Services.Models
{
    public class IdentityGatewayApiSettingModel
    {
        public string UserId { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public string Value { get; set; }

        public string SettingKey { get; set; }
    }
}