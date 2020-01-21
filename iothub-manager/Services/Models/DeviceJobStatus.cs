// Copyright (c) Microsoft. All rights reserved.

namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    /// <summary>
    /// refer to Microsoft.Microsoft.Azure.Devices.DeviceJobStatus
    /// </summary>
    public enum DeviceJobStatus
    {
        Pending = 0,
        Scheduled = 1,
        Running = 2,
        Completed = 3,
        Failed = 4,
        Canceled = 5
    }
}
