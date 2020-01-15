using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeploymentStatus
    {
        Pending, Succeeded, Failed, Unknown
    }
}