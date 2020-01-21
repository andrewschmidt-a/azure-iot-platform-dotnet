using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.IoTHubManager.Services;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Mmm.Platform.IoT.IoTHubManager.WebService.v1.Models;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.v1.Controllers
{
    [Route("v1/[controller]")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class DeploymentsController : Controller
    {
        private readonly IDeployments deployments;

        public DeploymentsController(IDeployments deployments)
        {
            this.deployments = deployments;
        }

        /// <summary>Create a deployment targeting a particular group</summary>
        /// <param name="deployment">Deployment information</param>
        /// <returns>Deployment information and initial success metrics</returns>
        [HttpPost]
        [Authorize("CreateDeployments")]
        public async Task<DeploymentApiModel> PostAsync([FromBody] DeploymentApiModel deployment)
        {
            if (string.IsNullOrWhiteSpace(deployment.Name))
            {
                throw new InvalidInputException("Name must be provided");
            }

            if (string.IsNullOrWhiteSpace(deployment.DeviceGroupId))
            {
                throw new InvalidInputException("DeviceGroupId must be provided");
            }

            if (string.IsNullOrWhiteSpace(deployment.DeviceGroupName))
            {
                throw new InvalidInputException("DeviceGroupName must be provided");
            }

            if (string.IsNullOrWhiteSpace(deployment.DeviceGroupQuery))
            {
                throw new InvalidInputException("DeviceGroupQuery must be provided");
            }

            if (string.IsNullOrWhiteSpace(deployment.PackageContent))
            {
                throw new InvalidInputException("PackageContent must be provided");
            }

            if (deployment.PackageType.Equals(PackageType.DeviceConfiguration)
                && string.IsNullOrEmpty(deployment.ConfigType))
            {
                throw new InvalidInputException("Configuration type must be provided");
            }

            if (deployment.Priority < 0)
            {
                throw new InvalidInputException($"Invalid priority provided of {deployment.Priority}. " +
                                                "It must be non-negative");
            }

            return new DeploymentApiModel(await this.deployments.CreateAsync(deployment.ToServiceModel()));
        }

        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<DeploymentListApiModel> GetAsync()
        {
            return new DeploymentListApiModel(await this.deployments.ListAsync());
        }

        /// <summary>Get one deployment</summary>
        /// <param name="id">Deployment id</param>
        /// <param name="includeDeviceStatus">Whether to retrieve additional details regarding device status</param>
        /// <returns>Deployment information with metrics</returns>
        [HttpGet("{id}")]
        [Authorize("ReadAll")]
        public async Task<DeploymentApiModel> GetAsync(string id, [FromQuery] bool includeDeviceStatus = false)
        {
            return new DeploymentApiModel(await this.deployments.GetAsync(id, includeDeviceStatus));
        }

        [HttpDelete("{id}")]
        [Authorize("DeleteDeployments")]
        public async Task DeleteAsync(string id)
        {
            await this.deployments.DeleteAsync(id);
        }
    }
}
