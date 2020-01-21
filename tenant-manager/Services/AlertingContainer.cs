using System;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public class AlertingContainer : IAlertingContainer
    {
        public readonly ITenantContainer TenantContainer;
        public readonly IStreamAnalyticsHelper StreamAnalyticsHelper;
        public readonly IRunbookHelper RunbookHelper;
        private const string SaNameFormat = "sa-{0}";

        public AlertingContainer(
            ITenantContainer tenantContainer,
            IStreamAnalyticsHelper streamAnalyticsHelper,
            IRunbookHelper runbookHelper)
        {
            this.TenantContainer = tenantContainer;
            this.StreamAnalyticsHelper = streamAnalyticsHelper;
            this.RunbookHelper = runbookHelper;
        }

        public bool SaJobExists(StreamAnalyticsJobModel saJobModel)
        {
            return !string.IsNullOrEmpty(saJobModel.StreamAnalyticsJobName) && !string.IsNullOrEmpty(saJobModel.JobState);
        }

        public async Task<StreamAnalyticsJobModel> AddAlertingAsync(string tenantId)
        {
            StreamAnalyticsJobModel saJobModel = await this.GetAlertingAsync(tenantId);
            if (this.SaJobExists(saJobModel))
            {
                throw new Exception("The given tenant already has a deployed stream analytics job.");
            }

            TenantModel tenant = await this.GetTenantFromContainerAsync(tenantId);
            string saJobName = string.Format(SaNameFormat, tenantId.Substring(0, 8));
            await this.RunbookHelper.CreateAlerting(tenantId, saJobName, tenant.IotHubName);
            return new StreamAnalyticsJobModel
            {
                TenantId = tenant.TenantId,
                StreamAnalyticsJobName = saJobName,
                IsActive = false,
                JobState = "Creating"
            };
        }

        public async Task<StreamAnalyticsJobModel> RemoveAlertingAsync(string tenantId)
        {
            TenantModel tenant = await this.GetTenantFromContainerAsync(tenantId);
            await this.RunbookHelper.DeleteAlerting(tenantId, tenant.SAJobName);
            return new StreamAnalyticsJobModel
            {
                TenantId = tenant.TenantId,
                StreamAnalyticsJobName = tenant.SAJobName,
                IsActive = false,
                JobState = "Deleting"
            };
        }

        public async Task<StreamAnalyticsJobModel> GetAlertingAsync(string tenantId)
        {
            TenantModel tenant = await this.GetTenantFromContainerAsync(tenantId);
            try
            {
                var job = await this.StreamAnalyticsHelper.GetJobAsync(tenant.SAJobName);
                return new StreamAnalyticsJobModel
                {
                    TenantId = tenant.TenantId,
                    JobState = job.JobState,
                    IsActive = this.StreamAnalyticsHelper.JobIsActive(job),
                    StreamAnalyticsJobName = job.Name,
                };
            }
            catch (ResourceNotFoundException)
            {
                // Return a model with null information regarding the stream analytics job if it does not exist
                return new StreamAnalyticsJobModel
                {
                    TenantId = tenant.TenantId,
                    IsActive = false
                };
            }
            catch (Exception e)
            {
                throw new Exception("An Unknown exception occurred while attempting to get the tenant's stream analytics job.", e);
            }
        }

        public async Task<StreamAnalyticsJobModel> StartAlertingAsync(string tenantId)
        {
            StreamAnalyticsJobModel saJobModel = await this.GetAlertingAsync(tenantId);
            if (!this.SaJobExists(saJobModel))
            {
                throw new Exception("There is no StreamAnalyticsJob is available to start for this tenant.");
            }
            await this.StreamAnalyticsHelper.StartAsync(saJobModel.StreamAnalyticsJobName);
            return saJobModel;
        }

        public async Task<StreamAnalyticsJobModel> StopAlertingAsync(string tenantId)
        {
            StreamAnalyticsJobModel saJobModel = await this.GetAlertingAsync(tenantId);
            if (!this.SaJobExists(saJobModel))
            {
                throw new Exception("There is no StreamAnalyticsJob is available to stop for this tenant.");
            }
            await this.StreamAnalyticsHelper.StopAsync(saJobModel.StreamAnalyticsJobName);
            return saJobModel;
        }

        private async Task<TenantModel> GetTenantFromContainerAsync(string tenantId)
        {
            TenantModel tenant = await this.TenantContainer.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                throw new Exception("The given tenant does not exist.");
            }
            bool tenantReady = await this.TenantContainer.TenantIsReadyAsync(tenantId);
            if (!tenantReady)
            {
                throw new Exception("The tenant is not fully deployed yet. Please wait for the tenant to fully deploy before performing alerting operations");
            }
            return tenant;
        }
    }
}