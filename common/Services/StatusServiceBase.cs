using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services
{
    public class StatusServiceBase : IStatusService
    {
        private readonly AppConfig config;
        protected IDictionary<string, IStatusOperation> dependencies;

        public StatusServiceBase(AppConfig config)
        {
            this.config = config;
        }

        protected void SetServiceStatus(string dependencyName, StatusResultServiceModel serviceResult, StatusServiceModel result, IList<string> errors)
        {
            if (!serviceResult.IsHealthy)
            {
                errors.Add(dependencyName + " check failed");
                result.Status.IsHealthy = false;
            }

            result.Dependencies.Add(dependencyName, serviceResult);
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // Loop over the IStatusOperation classes and get each status - set service status based on each response
            foreach (var dependency in dependencies)
            {
                var service = dependency.Value;
                var serviceResult = await service.StatusAsync();
                SetServiceStatus(dependency.Key, serviceResult, result, errors);
            }

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }

            result.Properties.Add("AuthRequired", config.Global.AuthRequired.ToString());
            result.Properties.Add("Endpoint", config.ASPNETCORE_URLS);

            return result;
        }
    }
}
