using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using IdentityServer4.Extensions;
using System.Security.Claims;
using IdentityModel;
using IdentityGateway.Services.Runtime;
using Microsoft.AspNetCore.Http;
using System.Linq;
using IdentityGateway.Services.Models;

namespace IdentityGateway.Services.Helpers
{
    public interface IJWTHelper
    {
        Task<JwtSecurityToken> GetIdentityToken(List<Claim> claims, string tenant, string audience, DateTime? expiration);
        JwtSecurityToken MintToken(List<Claim> claims, string audience, DateTime expirationDateTime);
    }
    public class JWTHelper : IJWTHelper
    {
        private UserTenantContainer _userTenantContainer;
        private UserSettingsContainer _userSettingsContainer;
        private IServicesConfig _config;
        private IHttpContextAccessor _httpContextAccessor;
        public JWTHelper(UserTenantContainer userTenantContainer,
            UserSettingsContainer userSettingsContainer, IServicesConfig config, IHttpContextAccessor httpContextAccessor)
        {
            this._userTenantContainer = userTenantContainer;
            this._userSettingsContainer = userSettingsContainer;
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
        }

        public async Task<JwtSecurityToken> GetIdentityToken(List<Claim> claims, string tenant, string audience, DateTime? expiration)
        {
            //add iat claim
            var timeSinceEpoch = DateTime.UtcNow.ToEpochTime();
            claims.Add(new Claim("iat", timeSinceEpoch.ToString(), ClaimValueTypes.Integer));

            var userId = claims.First(t => t.Type == "sub").Value;
            // Create a userTenantInput for the purpose of finding the full tenant list associated with this user
            UserTenantInput tenantInput = new UserTenantInput
            {
                userId = userId
            };
            List<UserTenantModel> tenantList = await this._userTenantContainer.GetAllAsync(tenantInput);

            //User did not specify the tenant to log into so get the default or last used
            if (String.IsNullOrEmpty(tenant))
            {
                // authState has no tenant, so we should use either the User's last used tenant, or the first tenant available to them
                // Create a UserSettingsInput for the purpose of finding the LastUsedTenant setting for this user
                UserSettingsInput settingsInput = new UserSettingsInput
                {
                    userId = userId,
                    settingKey = "LastUsedTenant"
                };
                UserSettingsModel lastUsedSetting = await this._userSettingsContainer.GetAsync(settingsInput);
                if (lastUsedSetting != null)
                {

                    tenant = lastUsedSetting.Value;
                }

                if (String.IsNullOrEmpty(tenant) && tenantList.Count > 0)
                {
                    tenant =
                        tenantList.First()
                            .TenantId; // Set the tenant to the first tenant in the list of tenants for this user
                }
            }

            // If User not associated with Tenant then dont add claims return token without 
            if (tenant != null)
            {
                UserTenantInput input = new UserTenantInput
                {
                    userId = userId,
                    tenant = tenant
                };
                UserTenantModel tenantModel = await this._userTenantContainer.GetAsync(input);
                // Add Tenant
                claims.Add(new Claim("tenant", tenantModel.TenantId));
                // Add Roles
                tenantModel.RoleList.ForEach(role => claims.Add(new Claim("role", role)));
            }

            DateTime expirationDateTime = expiration ?? DateTime.Now.AddDays(30);
            // add all tenants they have access to
            claims.AddRange(tenantList.Select(t => new Claim("available_tenants", t.TenantId)));
            
            // Token to String so you can use it in your client
            return this.MintToken(claims, audience, expirationDateTime);
        }
        public JwtSecurityToken MintToken(List<Claim> claims, string audience, DateTime expirationDateTime)
        {

            string forwardedFor = null;
            // add issuer with forwarded for address if exists (added by reverse proxy)
            if (_httpContextAccessor.HttpContext.Request.Headers.Where(t => t.Key == "X-Forwarded-For").Count() > 0)
            {
                forwardedFor = _httpContextAccessor.HttpContext.Request.Headers.Where(t => t.Key == "X-Forwarded-For").FirstOrDefault().Value
                    .First();
            }

            // Create Security key  using private key above:
            // not that latest version of JWT using Microsoft namespace instead of System
            var securityKey =
                new RsaSecurityKey(IdentityGateway.Services.Helpers.RSA.DecodeRSA(_config.PrivateKey));

            // Also note that securityKey length should be >256b
            // so you have to make sure that your private key has a proper length
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials
                (securityKey, SecurityAlgorithms.RsaSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: forwardedFor ?? "https://" + this._httpContextAccessor.HttpContext.Request.Host.ToString() + "/",
                audience: audience,
                expires: expirationDateTime.ToUniversalTime(),
                claims: claims.ToArray(),
                signingCredentials: credentials
            );
            return token;
        }
    }
}
