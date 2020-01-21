namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    /// <summary>
    /// refer to Microsoft.Microsoft.Azure.Devices.JobStatus
    /// </summary>
    public enum JobStatus
    {
        Unknown = 0,
        Enqueued = 1,
        Running = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5,
        Scheduled = 6,
        Queued = 7,
    }
}