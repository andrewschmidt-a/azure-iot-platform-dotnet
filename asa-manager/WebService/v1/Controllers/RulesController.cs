using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.AsaManager.WebService.v1;
using Mmm.Platform.IoT.AsaManager.WebService.v1.Models;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.Common.Services.Wrappers;

namespace Mmm.Platform.IoT.AsaManager.WebService.v1.Controllers
{
    [Route("v1/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class RulesController : Controller
    {
        private readonly RulesConverter _ruleConverter;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger _logger;

        public RulesController(
            RulesConverter ruleConverter,
            IKeyGenerator keyGenerator,
            ILogger<RulesController> logger)
        {
            this._ruleConverter = ruleConverter;
            this._keyGenerator = keyGenerator;
            this._logger = logger;
        }

        public void Forget(Task task, string operationId)
        {
            task.ContinueWith(
                t => { this._logger.LogError(t.Exception, "An exception occurred during the background conversion. OperationId {operationId}", operationId); },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        [HttpPost("")]
        public BeginConversionApiModel BeginRuleConversion()
        {
            string tenantId = this.GetTenantId();
            string operationId = this._keyGenerator.Generate();
            // This can be a long running process due to querying of cosmos/iothub - don't wait for it
            Forget(this._ruleConverter.ConvertAsync(tenantId, operationId), operationId);
            // Return the operationId of the rule conversion synchronous process
            return new BeginConversionApiModel
            {
                TenantId = tenantId,
                OperationId = operationId
            };
        }
    }
}
