using System;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.TenantManager.Services.Helpers;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public class AlertingContainer : IAlertingContainer
    {
        public readonly ITenantContainer _tenantContainer;
        public readonly IStreamAnalyticsHelper _streamAnalyticsHelper;
        public readonly IRunbookHelper _runbookHelper;
        private const string SA_NAME_FORMAT = "sa-{0}";

        public AlertingContainer(
            ITenantContainer tenantContainer,
            IStreamAnalyticsHelper streamAnalyticsHelper,
            IRunbookHelper RunbookHelper)
        {
            this._tenantContainer = tenantContainer;
            this._streamAnalyticsHelper = streamAnalyticsHelper;
            this._runbookHelper = RunbookHelper;
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
            string saJobName = string.Format(SA_NAME_FORMAT, tenantId.Substring(0, 8));
            await this._runbookHelper.CreateAlerting(tenantId, saJobName, tenant.IotHubName);
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
            await this._runbookHelper.DeleteAlerting(tenantId, tenant.SAJobName);
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
                var job = await this._streamAnalyticsHelper.GetJobAsync(tenant.SAJobName);
                return new StreamAnalyticsJobModel
                {
                    TenantId = tenant.TenantId,
                    JobState = job.JobState,
                    IsActive = this._streamAnalyticsHelper.JobIsActive(job),
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
            await this._streamAnalyticsHelper.StartAsync(saJobModel.StreamAnalyticsJobName);
            return saJobModel;
        }

        public async Task<StreamAnalyticsJobModel> StopAlertingAsync(string tenantId)
        {
            StreamAnalyticsJobModel saJobModel = await this.GetAlertingAsync(tenantId);
            if (!this.SaJobExists(saJobModel))
            {
                throw new Exception("There is no StreamAnalyticsJob is available to stop for this tenant.");
            }
            await this._streamAnalyticsHelper.StopAsync(saJobModel.StreamAnalyticsJobName);
            return saJobModel;
        }

        private async Task<TenantModel> GetTenantFromContainerAsync(string tenantId)
        {
            TenantModel tenant = await this._tenantContainer.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                throw new Exception("The given tenant does not exist.");
            }
            bool tenantReady = await this._tenantContainer.TenantIsReadyAsync(tenantId);
            if (!tenantReady)
            {
                throw new Exception("The tenant is not fully deployed yet. Please wait for the tenant to fully deploy before performing alerting operations");
            }
            return tenant;
        }
    }
}