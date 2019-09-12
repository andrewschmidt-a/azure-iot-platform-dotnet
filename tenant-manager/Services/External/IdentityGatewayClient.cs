using System;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Http;
using HttpRequest = Microsoft.Azure.IoTSolutions.TenantManager.Services.Http.HttpRequest;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Models;

namespace Microsoft.Azure.IoTSolutions.TenantManager.Services.External
{
    public class IdentityGatewayClient : IIdentityGatewayClient
    {
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string URI_KEY = "ExternalDependencies:identitygatewaywebserviceurl";
        private const string AZDS_ROUTE_KEY = "azds-route-as";
        private readonly IHttpClient httpClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly string serviceUri;

        public IdentityGatewayClient(IHttpClient httpClient, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            this.httpClient = httpClient;
            this.serviceUri = config[URI_KEY];
            this.httpContextAccessor = httpContextAccessor;
        }

        public async void addUserToTenantAsync(string userId, string tenantId, string roles)
        {
            HttpRequest request = request = CreateRequest($"tenants/{userId}", new IdentityGatewayApiModel { Roles = roles });
            
            request.Headers.Add(TENANT_HEADER, tenantId);

            await this.httpClient.PostAsync(request);
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

            if (this.httpContextAccessor.HttpContext.Request.Headers.ContainsKey(AZDS_ROUTE_KEY))
            {
                request.Headers.Add(AZDS_ROUTE_KEY, this.httpContextAccessor.HttpContext.Request.Headers.First(p => p.Key == AZDS_ROUTE_KEY).Value.First());
            }
            
            return request;
        }
    }
}