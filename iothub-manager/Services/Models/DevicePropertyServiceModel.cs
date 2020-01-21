using System.Collections.Generic;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    public class DevicePropertyServiceModel
    {
        public bool Rebuilding { get; set; } = false;

        public HashSet<string> Tags { get; set; }

        public HashSet<string> Reported { get; set; }

        public bool IsNullOrEmpty() => (Tags == null || Tags.Count == 0) && (Reported == null || Reported.Count == 0);
    }
}
