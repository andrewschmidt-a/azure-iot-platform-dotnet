using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.TenantManager.WebService.Models
{
    public class StatusResultModel
    {
        public StatusResultModel()
        {
        }

        public StatusResultModel(StatusResultServiceModel servicemodel)
        {
            this.IsHealthy = servicemodel.IsHealthy;
            this.Message = servicemodel.Message;
        }

        public bool IsHealthy { get; set; }

        public string Message { get; set; }
    }
}
