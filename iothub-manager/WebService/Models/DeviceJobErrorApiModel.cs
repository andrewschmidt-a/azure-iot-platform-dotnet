using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Models
{
    public class DeviceJobErrorApiModel
    {
        public DeviceJobErrorApiModel()
        {
        }

        public DeviceJobErrorApiModel(DeviceJobErrorServiceModel error)
        {
            this.Code = error.Code;
            this.Description = error.Description;
        }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }
}