using Mmm.Platform.IoT.IoTHubManager.Services.Models;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.v1.Models
{
    public class MethodResultApiModel : MethodResultServiceModel
    {
        public MethodResultApiModel(MethodResultServiceModel model)
        {
            this.Status = model.Status;
            this.JsonPayload = model.JsonPayload;
        }
    }
}
