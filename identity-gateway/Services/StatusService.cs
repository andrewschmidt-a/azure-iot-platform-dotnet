using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public class StatusService : IStatusService
    {
        public UserTenantContainer _userTenantContainer;
        public UserSettingsContainer _userSettingsContainer;
        private AppConfig config;

        public StatusService(AppConfig config, UserTenantContainer userTenantContainer, UserSettingsContainer userSettingsContainer)
        {
            this.config = config;
            this._userTenantContainer = userTenantContainer;
            this._userSettingsContainer = userSettingsContainer;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // Check connection to Table Storage
            // TODO: Add check of settings table as well
            var storageResult = await this._userTenantContainer.StatusAsync();
            SetServiceStatus("TableStorage", storageResult, result, errors);

            // Check Azure B2C instance
            StatusResultServiceModel azureB2CResult;

            try
            {
                string authUri = config.Global.AzureB2cBaseUri;
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(authUri);
                string responseMessage = "Alive and Well.";

                if (!response.IsSuccessStatusCode)
                {
                    responseMessage = $"It failed with a code of {response.StatusCode}.";
                }
                azureB2CResult = new StatusResultServiceModel(response.IsSuccessStatusCode, responseMessage);
            }
            catch (Exception E)
            {
                azureB2CResult = new StatusResultServiceModel(false, E.Message);
            }

            SetServiceStatus("AzureB2C", azureB2CResult, result, errors);

            result.Properties.Add("AuthRequired", config.Global.AuthRequired.ToString());
            result.Properties.Add("Endpoint", config.ASPNETCORE_URLS);

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }
            return result;
        }

        private void SetServiceStatus(
            string dependencyName,
            StatusResultServiceModel serviceResult,
            StatusServiceModel result,
            List<string> errors)
        {
            if (!serviceResult.IsHealthy)
            {
                errors.Add(dependencyName + " check failed");
                result.Status.IsHealthy = false;
            }
            result.Dependencies.Add(dependencyName, serviceResult);
        }
    }
}
