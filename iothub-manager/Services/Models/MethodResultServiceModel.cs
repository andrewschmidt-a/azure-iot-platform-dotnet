using Microsoft.Azure.Devices;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    public class MethodResultServiceModel
    {
        public MethodResultServiceModel()
        {
        }

        public MethodResultServiceModel(CloudToDeviceMethodResult result)
        {
            this.Status = result.Status;
            this.JsonPayload = result.GetPayloadAsJson();
        }

        public int Status { get; set; }

        public string JsonPayload { get; set; }
    }
}