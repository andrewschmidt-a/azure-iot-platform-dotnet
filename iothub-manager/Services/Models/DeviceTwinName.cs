using System.Collections.Generic;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    public class DeviceTwinName
    {
        public HashSet<string> Tags { get; set; }

        public HashSet<string> ReportedProperties { get; set; }
    }
}
