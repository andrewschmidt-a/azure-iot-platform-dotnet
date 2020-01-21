using Microsoft.Azure.Devices;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    public class DeviceJobErrorServiceModel
    {
        public DeviceJobErrorServiceModel(DeviceJobError error)
        {
            this.Code = error.Code;
            this.Description = error.Description;
        }

        public string Code { get; }

        public string Description { get; }
    }
}
