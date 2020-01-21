// <copyright file="JobsController.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.IoTHubManager.Services;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Mmm.Platform.IoT.IoTHubManager.WebService.Models;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Controllers
{
    [Route("v1/[controller]")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class JobsController : Controller
    {
        private readonly IJobs jobs;

        public JobsController(IJobs jobs)
        {
            this.jobs = jobs;
        }

        /// <summary>
        /// Get list of jobs by status/type
        /// </summary>
        /// <param name="jobType">The type of job</param>
        /// <param name="jobStatus">The status of job</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="from">Optional. The begin time of interesting period</param>
        /// <param name="to">Optional. The end time of interesting period</param>
        /// <returns>The list of jobs</returns>
        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<IEnumerable<JobApiModel>> GetAsync(
            [FromQuery] JobType? jobType,
            [FromQuery] JobStatus? jobStatus,
            [FromQuery] int? pageSize,
            [FromQuery] string from,
            [FromQuery] string to)
        {
            var result = await this.jobs.GetJobsAsync(jobType, jobStatus, pageSize, from, to);
            return result.Select(r => new JobApiModel(r));
        }

        /// <summary>
        /// Get job status by jobId
        /// </summary>
        /// <param name="jobId">The jobId</param>
        /// <param name="includeDeviceDetails">`true` for request per-device details</param>
        /// <param name="deviceJobStatus">The interesting device job status. `null` means no restrict</param>
        /// <returns>The job object</returns>
        [HttpGet("{jobId}")]
        [Authorize("ReadAll")]
        public async Task<JobApiModel> GetJobAsync(
            string jobId,
            [FromQuery]bool? includeDeviceDetails,
            [FromQuery]DeviceJobStatus? deviceJobStatus)
        {
            var result = await this.jobs.GetJobsAsync(jobId, includeDeviceDetails, deviceJobStatus);
            return new JobApiModel(result);
        }

        /// <summary>
        /// Schedule job
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("CreateJobs")]
        public async Task<JobApiModel> ScheduleAsync([FromBody] JobApiModel parameter)
        {
            if (parameter.UpdateTwin != null)
            {
                var result = await this.jobs.ScheduleTwinUpdateAsync(parameter.JobId, parameter.QueryCondition, parameter.UpdateTwin.ToServiceModel(), parameter.StartTimeUtc ?? DateTime.UtcNow, parameter.MaxExecutionTimeInSeconds ?? 0);
                return new JobApiModel(result);
            }

            if (parameter.MethodParameter != null)
            {
                var result = await this.jobs.ScheduleDeviceMethodAsync(parameter.JobId, parameter.QueryCondition, parameter.MethodParameter.ToServiceModel(), parameter.StartTimeUtc ?? DateTime.UtcNow, parameter.MaxExecutionTimeInSeconds ?? 0);
                return new JobApiModel(result);
            }

            throw new NotSupportedException();
        }
    }
}