using System;
using System.Net.Http;
using Mmm.Platform.IoT.TenantManager.Services.Models;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.TenantManager.Services.External
{
    public class IdentityGatewayClient : IIdentityGatewayClient
    {
        private readonly IExternalRequestHelper _requestHelper;
        private readonly string serviceUri;

        public IdentityGatewayClient(AppConfig config, IExternalRequestHelper requestHelper)
        {
            this.serviceUri = config.ExternalDependencies.IdentityGatewayServiceUrl;
            this._requestHelper = requestHelper;
        }

        public string RequestUrl(string path)
        {
            return $"{this.serviceUri}/{path}";
        }

        /// <summary>
        /// Ping the IdentityGateway for its status
        /// </summary>
        /// <returns></returns>
        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                string url = this.RequestUrl("status/");
                var result = await this._requestHelper.ProcessRequestAsync<StatusServiceModel>(HttpMethod.Get, url);
                if (result == null || result.Status == null || !result.Status.IsHealthy)
                {
                    // bad status
                    return new StatusResultServiceModel(false, result.Status.Message);
                }
                else
                {
                    return new StatusResultServiceModel(true, "Alive and well!");
                }
            }
            catch (JsonReaderException)
            {
                return new StatusResultServiceModel(false, $"Unable to read the response from the IdentityGateway Status. The service may be down.");
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, $"Unable to get IdentityGateway Status: {e.Message}");
            }
        }

        /// <summary>
        /// Add a user-tenant relationship between the given userId and tenantId, with the given roles
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tenantId"></param>
        /// <param name="Roles"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiModel> addTenantForUserAsync(string userId, string tenantId, string Roles)
        {
            IdentityGatewayApiModel bodyContent = new IdentityGatewayApiModel(Roles);
            string url = this.RequestUrl($"tenants/{userId}");
            return await this._requestHelper.ProcessRequestAsync(HttpMethod.Post, url, bodyContent, tenantId);
        }

        /// <summary>
        /// get a user-tenant relationship for the given user and tenant Ids
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiModel> getTenantForUserAsync(string userId, string tenantId)
        {
            string url = this.RequestUrl($"tenants/{userId}");
            return await this._requestHelper.ProcessRequestAsync<IdentityGatewayApiModel>(HttpMethod.Get, url, tenantId);
        }

        /// <summary>
        /// delete all user-tenant relationships for the given tenantId
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiModel> deleteTenantForAllUsersAsync(string tenantId)
        {
            string url = this.RequestUrl($"tenants/all");
            return await this._requestHelper.ProcessRequestAsync<IdentityGatewayApiModel>(HttpMethod.Delete, url, tenantId);
        }

        /// <summary>
        /// get the userSetting for the given settingKey
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="settingKey"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiSettingModel> getSettingsForUserAsync(string userId, string settingKey)
        {
            string url = this.RequestUrl($"settings/{userId}/{settingKey}");
            return await this._requestHelper.ProcessRequestAsync<IdentityGatewayApiSettingModel>(HttpMethod.Get, url);
        }

        /// <summary>
        /// Create a new userSetting of the given key and value
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="settingKey"></param>
        /// <param name="settingValue"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiSettingModel> addSettingsForUserAsync(string userId, string settingKey, string settingValue)
        {
            string url = this.RequestUrl($"settings/{userId}/{settingKey}/{settingValue}");
            return await this._requestHelper.ProcessRequestAsync<IdentityGatewayApiSettingModel>(HttpMethod.Post, url);
        }

        /// <summary>
        /// Change a userSetting for the given key, to the given value
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="settingKey"></param>
        /// <param name="settingValue"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiSettingModel> updateSettingsForUserAsync(string userId, string settingKey, string settingValue)
        {
            string url = this.RequestUrl($"settings/{userId}/{settingKey}/{settingValue}");
            return await this._requestHelper.ProcessRequestAsync<IdentityGatewayApiSettingModel>(HttpMethod.Put, url);
        }
    }
}