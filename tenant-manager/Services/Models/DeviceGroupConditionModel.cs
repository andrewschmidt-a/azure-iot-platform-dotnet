using Newtonsoft.Json;

namespace Mmm.Platform.IoT.TenantManager.Services.Models
{
    public class DeviceGroupConditionModel
    {
        public string Field { get; set; }

        public string Operator { get; set; }

        public string Value { get; set; }
    }
}
