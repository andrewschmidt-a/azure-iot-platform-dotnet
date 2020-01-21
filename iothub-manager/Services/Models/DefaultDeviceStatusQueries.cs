using System.Collections.Generic;
using static Mmm.Platform.IoT.Config.Services.Models.DeviceStatusQueries;

namespace Mmm.Platform.IoT.Config.Services.Models
{
    public class DefaultDeviceStatusQueries
    {
        public static IDictionary<QueryType, string> Queries { get; set; } = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, @"SELECT deviceId from devices where configurations.[[{0}]].status = 'Applied'" },
            { QueryType.SUCCESSFUL, string.Empty },
            { QueryType.FAILED, string.Empty }
        };
    }
}
