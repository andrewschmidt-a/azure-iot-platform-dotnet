using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
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
        private const string URI_KEY = "ExternalDependencies:identitygatewaywebserviceurl";
        private const string AZDS_ROUTE_KEY = "azds-route-as";
        private readonly IHttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string serviceUri;
        private readonly string[] authenticatedRoles = { "admin" };
        private delegate Task<IHttpResponse> requestMethod(HttpRequest request);

        public IdentityGatewayClient(IServicesConfig config, IHttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            this.serviceUri = config.IdentityGatewayWebServiceUrl;
            this._httpClient = httpClient;
            this._httpContextAccessor = httpContextAccessor;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            HttpRequest request = CreateRequest("status/", new IdentityGatewayApiModel() { });
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

        public async Task<IdentityGatewayApiModel> addTenantForUserAsync(string userId, string tenantId, string Roles)
        {
            HttpRequest request = CreateRequest($"tenants/{userId}", new IdentityGatewayApiModel(Roles) { });
            request.Headers.Add(TENANT_HEADER, tenantId);
            return await this.processApiModelRequest<IdentityGatewayApiModel>(this._httpClient.PostAsync, request);
        }

        public async Task<IdentityGatewayApiModel> getTenantForUserAsync(string userId, string tenantId)
        {
            HttpRequest request = CreateRequest($"tenants/{userId}", new IdentityGatewayApiModel { });
            request.Headers.Add(TENANT_HEADER, tenantId);
            return await this.processApiModelRequest<IdentityGatewayApiModel>(this._httpClient.GetAsync, request);
        }

        public async Task<bool> isUserAuthenticated(string userId, string tenantId)
        {
            HttpRequest request = CreateRequest($"tenants/{userId}", new IdentityGatewayApiModel { });
            request.Headers.Add(TENANT_HEADER, tenantId);
            IdentityGatewayApiModel identityModel = await this.processApiModelRequest<IdentityGatewayApiModel>(this._httpClient.GetAsync, request);
            // return true if any roles are authenticated for - otherwise false
            return identityModel.RoleList.Any(role => authenticatedRoles.Contains(role));
        }
        
        public async Task<IdentityGatewayApiModel> deleteTenantForAllUsersAsync(string tenantId)
        {
            HttpRequest request = CreateRequest($"tenants/", new IdentityGatewayApiModel { });
            request.Headers.Add(TENANT_HEADER, tenantId);
            return await this.processApiModelRequest<IdentityGatewayApiModel>(this._httpClient.DeleteAsync, request);
        }

        public async Task<IdentityGatewayApiSettingModel> getSettingsForUserAsync(string userId, string settingKey)
        {
            HttpRequest request = CreateRequest($"settings/{userId}/{settingKey}", new IdentityGatewayApiModel { });
            return await this.processApiModelRequest<IdentityGatewayApiSettingModel>(this._httpClient.GetAsync, request);
        }

        public async Task<IdentityGatewayApiSettingModel> addSettingsForUserAsync(string userId, string settingKey, string settingValue)
        {
            HttpRequest request = CreateRequest($"settings/{userId}/{settingKey}/{settingValue}", new IdentityGatewayApiModel { });
            return await this.processApiModelRequest<IdentityGatewayApiSettingModel>(this._httpClient.GetAsync, request);
        }

        public async Task<IdentityGatewayApiSettingModel> updateSettingsForUserAsync(string userId, string settingKey, string settingValue)
        {
            HttpRequest request = CreateRequest($"settings/{userId}/{settingKey}/{settingValue}", new IdentityGatewayApiModel { });
            return await this.processApiModelRequest<IdentityGatewayApiSettingModel>(this._httpClient.PutAsync, request);
        }

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

        private HttpRequest CreateRequest(string path, IdentityGatewayApiModel content)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/{path}");

            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (content != null)
            {
                request.SetContent(content);
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