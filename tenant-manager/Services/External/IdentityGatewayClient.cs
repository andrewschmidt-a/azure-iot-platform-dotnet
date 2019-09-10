using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.IoTSolutions.TenantManager.Services.Http;

namespace Microsoft.Azure.IoTSolutions.TenantManager.Services.External
{
    public class IdentityGatewayClient : IIdentityGatewayClient
    {
        private const string TENANT_HEADER = "ApplicationTenantID";
        private readonly IHttpClient httpClient;
        private readonly string serviceUri;

        public IdentityGatewayClient(IHttpClient httpClient, IConfiguration config)
        {
            this.httpClient = httpClient;
            this.serviceUri = config["identityGatewayUri"];
        }

        public async void addUserToTenantAsync(string userId, string tenantId, string roles)
        {
            var request = new HttpRequest();

            request.SetUriFromString($"{this.serviceUri}/v1/tenants/{userId}");

            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (roles != null)
            {
                request.SetContent("{\"Roles\": \"" + roles + "\"}");
            }
            
            request.Headers.Add(TENANT_HEADER, tenantId);

            await this.httpClient.PostAsync(request);
        }
    }
}