// Copyright (c) Microsoft. All rights reserved.

namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    /// <summary>
    /// refer to Microsoft.Microsoft.Azure.Devices.JobType
    /// </summary>
    public enum JobType
    {
        Unknown = 0,
        ScheduleDeviceMethod = 3,
        ScheduleUpdateTwin = 4
    }
}
