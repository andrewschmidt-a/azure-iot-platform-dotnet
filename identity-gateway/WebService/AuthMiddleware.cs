// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using IdentityGateway.AuthUtils;

namespace IdentityGateway.WebService
{
    /// <summary>
    /// Insert into the HttpContext a tenant coming from the header. (assumes a client 2 client call
    /// </summary>
    public class AuthMiddleware
    {
        // Where to pull the tenant information from
        private const string TENANT_HEADER = "ApplicationTenantID";
        
        
        // Where to store the information in HTTPContext
        private const string TENANT_KEY = "TenantID";

        private RequestDelegate requestDelegate;

        public AuthMiddleware(RequestDelegate requestDelegate)
        {
            this.requestDelegate = requestDelegate;
        }

        public Task Invoke(HttpContext context)
        {
            string tenantId = context.Request.Headers[TENANT_HEADER].ToString();
         
            context.Request.SetTenant(tenantId);
            return this.requestDelegate(context);
        }
    }
}
