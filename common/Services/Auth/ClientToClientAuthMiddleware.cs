// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Mmm.Platform.IoT.Common.Services.Auth
{
    /// <summary>
    /// Insert into the HttpContext a tenant coming from the header. (assumes a client 2 client call
    /// </summary>
    public class ClientToClientAuthMiddleware
    {
        // Where to pull the tenant information from
        private const string TENANT_HEADER = "ApplicationTenantID";


        // Where to store the information in HTTPContext
        private const string TENANT_KEY = "TenantID";

        private RequestDelegate requestDelegate;

        private readonly ILogger _logger;
        public ClientToClientAuthMiddleware(RequestDelegate requestDelegate, ILogger<ClientToClientAuthMiddleware> logger)
        {
            this.requestDelegate = requestDelegate;
            _logger = logger;
        }

        public Task Invoke(HttpContext context)
        {
            string tenantId = context.Request.Headers[TENANT_HEADER].ToString();

            context.Request.SetTenant(tenantId);
            return this.requestDelegate(context);

        }
    }
}
