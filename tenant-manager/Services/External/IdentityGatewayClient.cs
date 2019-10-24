using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.Auth;
using MMM.Azure.IoTSolutions.TenantManager.Services.Http;
using HttpRequest = MMM.Azure.IoTSolutions.TenantManager.Services.Http.HttpRequest;
using ILogger = MMM.Azure.IoTSolutions.TenantManager.Services.Diagnostics.ILogger;
using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.External
{
    public class IdentityGatewayClient : IIdentityGatewayClient
    {
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string AZDS_ROUTE_KEY = "azds-route-as";
        
        private readonly IHttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string serviceUri;
        private delegate Task<IHttpResponse> requestMethod(HttpRequest request);

        public IdentityGatewayClient(IServicesConfig config, IHttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            this.serviceUri = config.IdentityGatewayWebServiceUrl;
            this._httpClient = httpClient;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Ping the IdentityGateway for its status
        /// </summary>
        /// <returns></returns>
        public async Task<StatusResultServiceModel> StatusAsync()
        {
            HttpRequest request = CreateRequest("status/");
            try
            {
                StatusServiceModel result = await this.processApiModelRequest<StatusServiceModel>(this._httpClient.GetAsync, request);
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
            HttpRequest request = CreateRequest($"tenants/{userId}", tenantId);
            request.SetContent(new IdentityGatewayApiModel(Roles));
            return await this.processApiModelRequest<IdentityGatewayApiModel>(this._httpClient.PostAsync, request);
        }

        /// <summary>
        /// get a user-tenant relationship for the given user and tenant Ids
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiModel> getTenantForUserAsync(string userId, string tenantId)
        {
            HttpRequest request = CreateRequest($"tenants/{userId}", tenantId);
            return await this.processApiModelRequest<IdentityGatewayApiModel>(this._httpClient.GetAsync, request);
        }
        
        /// <summary>
        /// delete all user-tenant relationships for the given tenantId
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiModel> deleteTenantForAllUsersAsync(string tenantId)
        {
            HttpRequest request = CreateRequest($"tenants/all", tenantId);
            return await this.processApiModelRequest<IdentityGatewayApiModel>(this._httpClient.DeleteAsync, request);
        }

        /// <summary>
        /// get the userSetting for the given settingKey
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="settingKey"></param>
        /// <returns></returns>
        public async Task<IdentityGatewayApiSettingModel> getSettingsForUserAsync(string userId, string settingKey)
        {
            HttpRequest request = CreateRequest($"settings/{userId}/{settingKey}");
            return await this.processApiModelRequest<IdentityGatewayApiSettingModel>(this._httpClient.GetAsync, request);
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
            HttpRequest request = CreateRequest($"settings/{userId}/{settingKey}/{settingValue}");
            return await this.processApiModelRequest<IdentityGatewayApiSettingModel>(this._httpClient.PostAsync, request);
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
            HttpRequest request = CreateRequest($"settings/{userId}/{settingKey}/{settingValue}");
            return await this.processApiModelRequest<IdentityGatewayApiSettingModel>(this._httpClient.PutAsync, request);
        }

        /// <summary>
        /// Process an IdentityGateway request for the IdentityGateway and transform the response to Model T
        /// </summary>
        /// <param name="method"></param>
        /// <param name="request"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private async Task<T> processApiModelRequest<T>(requestMethod method, HttpRequest request)
        {
            IHttpResponse response = null;
            try
            {
                response = await method(request);
                if (response == null || response.Content == null)
                {
                    throw new Exception("Http Request returned a null response.");
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Http Request returned a status code other than 200.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while sending the request.", e);
            }

            try
            {
                var responseContent = response.Content.ToString();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception e)
            {
                throw new JsonReaderException("Unable to deserialize response content to the proper API model.", e);
            }
        }

        /// <summary>
        /// Create an HttpRequest with the necessary parameters for an IdentityGateway API request
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        private HttpRequest CreateRequest(string path, string tenantId = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/{path}");
            
            string headerTenantId = tenantId ?? this._httpContextAccessor.HttpContext.Request.GetTenant();
            request.AddHeader(TENANT_HEADER, headerTenantId);

            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (this._httpContextAccessor.HttpContext.Request.Headers.ContainsKey(AZDS_ROUTE_KEY))
            {
                try
                {
                    var azdsRouteAs = this._httpContextAccessor.HttpContext.Request.Headers.First(p => String.Equals(p.Key, AZDS_ROUTE_KEY, StringComparison.OrdinalIgnoreCase));
                    request.Headers.Add(AZDS_ROUTE_KEY, azdsRouteAs.Value.First());  // azdsRouteAs.Value returns an iterable of strings, take the first
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to attach the {AZDS_ROUTE_KEY} header to the IdentityGatewayClient Request.", e);
                }
            }

            return request;
        }
    }
}