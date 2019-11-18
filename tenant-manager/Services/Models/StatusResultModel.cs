using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Models
{
    public class StatusResultModel
    {
        [JsonProperty(PropertyName = "IsHealthy", Order = 10)]
        public bool IsHealthy { get; set; }

        [JsonProperty(PropertyName = "Message", Order = 20)]
        public string Message { get; set; }

        public StatusResultModel(StatusResultServiceModel servicemodel)
        {
            this.IsHealthy = servicemodel.IsHealthy;
            this.Message = servicemodel.Message;
        }
    }
}
