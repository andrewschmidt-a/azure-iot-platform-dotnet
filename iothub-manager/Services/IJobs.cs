using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using DeviceJobStatus = Mmm.Platform.IoT.IoTHubManager.Services.Models.DeviceJobStatus;
using JobStatus = Mmm.Platform.IoT.IoTHubManager.Services.Models.JobStatus;
using JobType = Mmm.Platform.IoT.IoTHubManager.Services.Models.JobType;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public interface IJobs
    {
        Task<IEnumerable<JobServiceModel>> GetJobsAsync(
            JobType? jobType,
            JobStatus? jobStatus,
            int? pageSize,
            string queryFrom,
            string queryTo);

        Task<JobServiceModel> GetJobsAsync(
            string jobId,
            bool? includeDeviceDetails,
            DeviceJobStatus? deviceJobStatus);

        Task<JobServiceModel> ScheduleDeviceMethodAsync(
            string jobId,
            string queryCondition,
            MethodParameterServiceModel parameter,
            DateTimeOffset startTimeUtc,
            long maxExecutionTimeInSeconds);

        Task<JobServiceModel> ScheduleTwinUpdateAsync(
            string jobId,
            string queryCondition,
            TwinServiceModel twin,
            DateTimeOffset startTimeUtc,
            long maxExecutionTimeInSeconds);
    }
}
