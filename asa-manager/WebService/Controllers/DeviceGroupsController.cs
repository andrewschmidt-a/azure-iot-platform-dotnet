using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.Common.Services.Wrappers;

namespace Mmm.Platform.IoT.AsaManager.WebService.Controllers
{
    [Route("v1/[controller]")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class DeviceGroupsController : Controller
    {
        private readonly IConverter deviceGroupConverter;
        private readonly IKeyGenerator keyGenerator;
        private readonly ILogger logger;

        public DeviceGroupsController(
            IConverter devicegroupConverter,
            IKeyGenerator keyGenerator,
            ILogger<DeviceGroupsController> logger)
        {
            this.deviceGroupConverter = devicegroupConverter;
            this.keyGenerator = keyGenerator;
            this.logger = logger;
        }

        [HttpPost("")]
        public BeginConversionApiModel BeginDeviceGroupConversion()
        {
            string tenantId = this.GetTenantId();
            string operationId = this.keyGenerator.Generate();

            // This can be a long running process due to querying of cosmos/iothub - don't wait for itsyn
            Forget(this.deviceGroupConverter.ConvertAsync(tenantId, operationId), operationId);

            // Return the operationId of the devicegroup conversion synchronous process
            return new BeginConversionApiModel
            {
                TenantId = tenantId,
                OperationId = operationId,
            };
        }

        private void Forget(Task task, string operationId)
        {
            task.ContinueWith(
                t => { this.logger.LogError(t.Exception, "An exception occurred during the background conversion. OperationId {operationId}", operationId); },
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}