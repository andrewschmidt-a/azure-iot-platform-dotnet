using System.Collections.Generic;
using static Mmm.Platform.IoT.Config.Services.Models.DeviceStatusQueries;

namespace Mmm.Platform.IoT.Config.Services.Models
{
    public class FirmwareStatusQueries
    {
        public static IDictionary<QueryType, string> Queries { get; set; } = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, @"SELECT deviceId from devices where configurations.[[{0}]].status = 'Applied'" },
            { QueryType.SUCCESSFUL, @"SELECT deviceId FROM devices WHERE configurations.[[{0}]].status = 'Applied' AND properties.reported.firmware.fwUpdateStatus='Current' AND properties.reported.firmware.type='IoTDevKit'" },
            { QueryType.FAILED, @"SELECT deviceId FROM devices WHERE configurations.[[{0}]].status = 'Applied' AND properties.reported.firmware.fwUpdateStatus='Error' AND properties.reported.firmware.type='IoTDevKit'" }
        };
    }
}
