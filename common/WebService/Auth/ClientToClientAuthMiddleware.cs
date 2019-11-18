// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.AuthUtils;
using Mmm.Platform.IoT.Common.Services.Diagnostics;

namespace Mmm.Platform.IoT.Common.WebService.Auth
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

        private readonly ILogger log;
        public ClientToClientAuthMiddleware(RequestDelegate requestDelegate, ILogger log)
        {
            this.requestDelegate = requestDelegate;
            this.log = log;
        }

        public Task Invoke(HttpContext context)
        {
            string tenantId = context.Request.Headers[TENANT_HEADER].ToString();
         
            context.Request.SetTenant(tenantId);
            return this.requestDelegate(context);

        }
    }
}
