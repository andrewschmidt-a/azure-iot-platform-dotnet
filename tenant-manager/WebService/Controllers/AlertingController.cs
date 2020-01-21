using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.TenantManager.Services;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.WebService.Controllers
{
    [Route("api/[controller]")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class AlertingController : Controller
    {
        private readonly IAlertingContainer _alertingContainer;
        private readonly ILogger _logger;

        public AlertingController(IAlertingContainer alertingContainer, ILogger<AlertingController> log)
        {
            this._alertingContainer = alertingContainer;
            this._logger = log;
        }

        [HttpPost]
        [Authorize("EnableAlerting")]
        public async Task<StreamAnalyticsJobModel> AddAlertingAsync()
        {
            return await this._alertingContainer.AddAlertingAsync(this.GetTenantId());
        }

        [HttpDelete]
        [Authorize("DisableAlerting")]
        public async Task<StreamAnalyticsJobModel> RemoveAlertingAsync()
        {
            return await this._alertingContainer.RemoveAlertingAsync(this.GetTenantId());
        }

        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<StreamAnalyticsJobModel> GetAlertingAsync([FromQuery] bool createIfNotExists = false)
        {
            string tenantId = this.GetTenantId();
            StreamAnalyticsJobModel model = await this._alertingContainer.GetAlertingAsync(tenantId);
            if (!this._alertingContainer.SaJobExists(model) && createIfNotExists)
            {
                // If the tenant does not have an sa job, start creating it
                _logger.LogInformation("The tenant does not already have alerting enabled and the createIfNotExists parameter was set to true. Creating a stream analytics job now. TenantId: {tenantId}", tenantId);
                return await this._alertingContainer.AddAlertingAsync(tenantId);
            }
            else
            {
                return model;
            }
        }

        [HttpPost("start")]
        [Authorize("EnableAlerting")]
        public async Task<StreamAnalyticsJobModel> StartAsync()
        {
            return await this._alertingContainer.StartAlertingAsync(this.GetTenantId());
        }

        [HttpPost("stop")]
        [Authorize("DisableAlerting")]
        public async Task<StreamAnalyticsJobModel> StopAsync()
        {
            return await this._alertingContainer.StopAlertingAsync(this.GetTenantId());
        }
    }
}
