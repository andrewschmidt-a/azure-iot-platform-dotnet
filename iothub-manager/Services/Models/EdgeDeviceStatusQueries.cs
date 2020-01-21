// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using static Mmm.Platform.IoT.Config.Services.Models.DeviceStatusQueries;

namespace Mmm.Platform.IoT.Config.Services.Models
{
    public class EdgeDeviceStatusQueries
    {
        public static IDictionary<QueryType, string> Queries = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, @"SELECT deviceId from devices.modules WHERE 
                moduleId = '$edgeAgent' 
                AND configurations.[[{0}]].status = 'Applied'" },
            { QueryType.SUCCESSFUL, @"SELECT deviceId from devices.modules WHERE 
                moduleId = '$edgeAgent' 
                AND configurations.[[{0}]].status = 'Applied' 
                AND properties.desired.$version = properties.reported.lastDesiredVersion  
                AND properties.reported.lastDesiredStatus.code = 200" },
            { QueryType.FAILED, @"SELECT deviceId FROM devices.modules WHERE 
                moduleId = '$edgeAgent' 
                AND configurations.[[{0}]].status = 'Applied' 
                AND properties.desired.$version = properties.reported.lastDesiredVersion 
                AND properties.reported.lastDesiredStatus.code != 200" }
        };
    }
}
