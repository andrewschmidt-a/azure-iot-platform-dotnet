// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Azure.IoTSolutions.Auth
{
    public static class RequestExtension
    {
        private const string CONTEXT_KEY_USER_CLAIMS = "CurrentUserClaims";
        private const string CONTEXT_KEY_AUTH_REQUIRED = "AuthRequired";
        private const string CONTEXT_KEY_ALLOWED_ACTIONS = "CurrentUserAllowedActions";
        private const string CONTEXT_KEY_EXTERNAL_REQUEST = "ExternalRequest";

        private const string CONTEXT_KEY_TENANT_ID = "TenantID";

        private const string CLAIM_KEY_TENANT_ID = "tenant";
        private const string HEADER_KEY_TENANT_ID = "ApplicationTenantID";
        // Role claim type
        private const string ROLE_CLAIM_TYPE = "role";
        private const string USER_OBJECT_ID_CLAIM_TYPE = "sub";

        // Store the current user claims in the current request
        public static void SetCurrentUserClaims(this HttpRequest request, IEnumerable<Claim> claims)
        {
            request.HttpContext.Items[CONTEXT_KEY_USER_CLAIMS] = claims;
        }

        // Get the user claims from the current request
        public static IEnumerable<Claim> GetCurrentUserClaims(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(CONTEXT_KEY_USER_CLAIMS))
            {
                return new List<Claim>();
            }

            return request.HttpContext.Items[CONTEXT_KEY_USER_CLAIMS] as IEnumerable<Claim>;
        }
        // Store authentication setting in the current request
        public static void SetAuthRequired(this HttpRequest request, bool authRequired)
        {
            request.HttpContext.Items[CONTEXT_KEY_AUTH_REQUIRED] = authRequired;
        }

        // Get the authentication setting in the current request
        public static bool GetAuthRequired(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(CONTEXT_KEY_AUTH_REQUIRED))
            {
                return true;
            }

            return (bool)request.HttpContext.Items[CONTEXT_KEY_AUTH_REQUIRED];
        }

        // Store source of request in the current request
        public static void SetExternalRequest(this HttpRequest request, bool external)
        {
            request.HttpContext.Items[CONTEXT_KEY_EXTERNAL_REQUEST] = external;
        }

        // Get the source of request in the current request
        public static bool IsExternalRequest(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(CONTEXT_KEY_EXTERNAL_REQUEST))
            {
                return true;
            }

            return (bool)request.HttpContext.Items[CONTEXT_KEY_EXTERNAL_REQUEST];
        }

        // Get the user's role claims from the current request
        public static string GetCurrentUserObjectId(this HttpRequest request)
        {
            var claims = GetCurrentUserClaims(request);
            return claims.Where(c => c.Type.ToLowerInvariant().Equals(USER_OBJECT_ID_CLAIM_TYPE, StringComparison.CurrentCultureIgnoreCase))
                .Select(c => c.Value).First();
        }

        // Get the user's role claims from the current request
        public static IEnumerable<string> GetCurrentUserRoleClaim(this HttpRequest request)
        {
            var claims = GetCurrentUserClaims(request);
            return claims.Where(c => c.Type.ToLowerInvariant().Equals(ROLE_CLAIM_TYPE, StringComparison.CurrentCultureIgnoreCase))
                .Select(c => c.Value);
        }

        // Store the current user allowed actions in the current request
        public static void SetCurrentUserAllowedActions(this HttpRequest request, IEnumerable<string> allowedActions)
        {
            request.HttpContext.Items[CONTEXT_KEY_ALLOWED_ACTIONS] = allowedActions;
        }

        // Get the user's allowed actions from the current request
        public static IEnumerable<string> GetCurrentUserAllowedActions(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(CONTEXT_KEY_ALLOWED_ACTIONS))
            {
                return new List<string>();
            }

            return request.HttpContext.Items[CONTEXT_KEY_ALLOWED_ACTIONS] as IEnumerable<string>;
        }

        // Get the user's Tenant 
        public static string GetTenant(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(CONTEXT_KEY_TENANT_ID))
            {
                return null;
            }
            return request.HttpContext.Items[CONTEXT_KEY_TENANT_ID] as string;
            
        }
        // Set the user's Tenant  based off request
        public static void SetTenant(this HttpRequest request)
        {
            string tenantId = null;
            if (IsExternalRequest(request)) // If external then get from claims
            {
                if (GetCurrentUserClaims(request).All(t => t.Type != CLAIM_KEY_TENANT_ID))
                {
                    throw new Exception(CLAIM_KEY_TENANT_ID + " claim not found");
                }

                tenantId = GetCurrentUserClaims(request).First(t => t.Type == CLAIM_KEY_TENANT_ID).Value;
            }
            else // service to service -- get from Header
            {
                if (!request.Headers.ContainsKey(HEADER_KEY_TENANT_ID))
                {
                    throw new Exception(HEADER_KEY_TENANT_ID + " header not found");
                }

                tenantId = request.Headers[HEADER_KEY_TENANT_ID];
            }

            SetTenant(request, tenantId);


            return; 
        }
        // Set the user's Tenant  from string
        public static void SetTenant(this HttpRequest request, string tenantId)
        {
            request.HttpContext.Items.Add(new KeyValuePair<object, object>(CONTEXT_KEY_TENANT_ID, tenantId));


            return; 
        }
    }
}
