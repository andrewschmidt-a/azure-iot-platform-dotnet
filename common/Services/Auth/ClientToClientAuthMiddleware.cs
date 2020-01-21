using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Mmm.Platform.IoT.Common.Services.Auth
{
    public class ClientToClientAuthMiddleware
    {
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string TENANT_KEY = "TenantID";
        private readonly ILogger logger;
        private RequestDelegate requestDelegate;

        public ClientToClientAuthMiddleware(RequestDelegate requestDelegate, ILogger<ClientToClientAuthMiddleware> logger)
        {
            this.requestDelegate = requestDelegate;
            this.logger = logger;
        }

        public Task Invoke(HttpContext context)
        {
            string tenantId = context.Request.Headers[TENANT_HEADER].ToString();

            context.Request.SetTenant(tenantId);
            return this.requestDelegate(context);

        }
    }
}
