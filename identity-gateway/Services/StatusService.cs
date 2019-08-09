using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace IdentityGateway.Services
{
    public class StatusService : IStatusService
    {
        private IConfiguration _config;
        public UserTenantContainer _userTenantContainer;
        public UserSettingsContainer _userSettingsContainer;
        
        public StatusService(IConfiguration config, UserTenantContainer userTenantContainer, UserSettingsContainer userSettingsContainer)
        {
            this._config = config;
            this._userTenantContainer = userTenantContainer;
            this._userSettingsContainer = userSettingsContainer;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // Check connection to Table Storage
            // TODO: Add check of settings table as well
            var storageResult = await this._userTenantContainer.PingAsync();
            SetServiceStatus("TableStorage", storageResult, result, errors);


            // Check Azure B2C instance
            StatusResultServiceModel resultTwo;

            try
            {
                var theURL = this._config["AzureB2CBaseUri"];
                HttpClient client = new HttpClient();
                var responseString = await client.GetAsync(theURL);
                var responseMessage = "Alive and Well.";

                if (!responseString.IsSuccessStatusCode)
                {
                    responseMessage = $"It failed with a code of {responseString.StatusCode}.";
                }
                resultTwo = new StatusResultServiceModel(responseString.IsSuccessStatusCode, responseMessage);
            }
            catch (Exception E)
            {
                resultTwo = new StatusResultServiceModel(false, E.Message);
            }

            SetServiceStatus("AzureB2C", resultTwo, result, errors);

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
            List<string> errors
            )
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
