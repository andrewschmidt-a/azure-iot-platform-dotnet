// <copyright file="RequestExtension.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Mmm.Platform.IoT.Common.Services
{
    public static class RequestExtension
    {
        public const string ContextKeyUserClaims = "CurrentUserClaims";
        private const string ContextKeyAuthRequired = "AuthRequired";
        private const string ContextKeyAllowedActions = "CurrentUserAllowedActions";
        private const string ContextKeyExternalRequest = "ExternalRequest";
        public const string ContextKeyTenantId = "TenantID";
        private const string ClaimKeyTenantId = "tenant";
        private const string HeaderKeyTenantId = "ApplicationTenantID";
        private const string RoleClaimType = "role";
        public const string UserObjectIdClaimType = "sub";

        public static void SetCurrentUserClaims(this HttpRequest request, IEnumerable<Claim> claims)
        {
            request.HttpContext.Items[ContextKeyUserClaims] = claims;
        }

        public static IEnumerable<Claim> GetCurrentUserClaims(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(ContextKeyUserClaims))
            {
                return new List<Claim>();
            }

            return request.HttpContext.Items[ContextKeyUserClaims] as IEnumerable<Claim>;
        }

        public static void SetAuthRequired(this HttpRequest request, bool authRequired)
        {
            request.HttpContext.Items[ContextKeyAuthRequired] = authRequired;
        }

        public static bool GetAuthRequired(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(ContextKeyAuthRequired))
            {
                return true;
            }

            return (bool)request.HttpContext.Items[ContextKeyAuthRequired];
        }

        public static void SetExternalRequest(this HttpRequest request, bool external)
        {
            request.HttpContext.Items[ContextKeyExternalRequest] = external;
        }

        public static bool IsExternalRequest(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(ContextKeyExternalRequest))
            {
                return true;
            }

            return (bool)request.HttpContext.Items[ContextKeyExternalRequest];
        }

        public static string GetCurrentUserObjectId(this HttpRequest request)
        {
            var claims = GetCurrentUserClaims(request);
            return claims.Where(c => c.Type.ToLowerInvariant().Equals(UserObjectIdClaimType, StringComparison.CurrentCultureIgnoreCase))
                .Select(c => c.Value).First();
        }

        public static IEnumerable<string> GetCurrentUserRoleClaim(this HttpRequest request)
        {
            var claims = GetCurrentUserClaims(request);
            return claims.Where(c => c.Type.ToLowerInvariant().Equals(RoleClaimType, StringComparison.CurrentCultureIgnoreCase))
                .Select(c => c.Value);
        }

        public static void SetCurrentUserAllowedActions(this HttpRequest request, IEnumerable<string> allowedActions)
        {
            request.HttpContext.Items[ContextKeyAllowedActions] = allowedActions;
        }

        public static IEnumerable<string> GetCurrentUserAllowedActions(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(ContextKeyAllowedActions))
            {
                return new List<string>();
            }

            return request.HttpContext.Items[ContextKeyAllowedActions] as IEnumerable<string>;
        }

        public static string GetTenant(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(ContextKeyTenantId))
            {
                return null;
            }

            return request.HttpContext.Items[ContextKeyTenantId] as string;
        }

        public static void SetTenant(this HttpRequest request)
        {
            string tenantId = null;
            if (IsExternalRequest(request))
            {
                if (GetCurrentUserClaims(request).Any(t => t.Type == ClaimKeyTenantId))
                {
                    tenantId = GetCurrentUserClaims(request).First(t => t.Type == ClaimKeyTenantId).Value;
                }
                else
                {
                    tenantId = null;
                }
            }
            else
            {
                if (request.Headers.ContainsKey(HeaderKeyTenantId))
                {
                    tenantId = request.Headers[HeaderKeyTenantId];
                }
                else
                {
                    tenantId = null;
                }
            }

            SetTenant(request, tenantId);
        }

        public static void SetTenant(this HttpRequest request, string tenantId)
        {
            request.HttpContext.Items.Add(new KeyValuePair<object, object>(ContextKeyTenantId, tenantId));
        }
    }
}
