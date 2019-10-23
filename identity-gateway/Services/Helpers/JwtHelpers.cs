using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityGateway.Services.Models;
using IdentityGateway.Services.Runtime;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace IdentityGateway.Services.Helpers
{
    public class JwtHelpers : IJwtHelpers
    {
        private UserTenantContainer _userTenantContainer;
        private UserSettingsContainer _userSettingsContainer;
        private IServicesConfig _config;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IOpenIdProviderConfiguration _openIdProviderConfiguration;
        private readonly IRsaHelpers _rsaHelpers;

        public JwtHelpers(UserTenantContainer userTenantContainer, UserSettingsContainer userSettingsContainer, IServicesConfig config, IHttpContextAccessor httpContextAccessor, IOpenIdProviderConfiguration openIdProviderConfiguration, IRsaHelpers rsaHelpers)
        {
            _userTenantContainer = userTenantContainer;
            _userSettingsContainer = userSettingsContainer;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _openIdProviderConfiguration = openIdProviderConfiguration;
            _rsaHelpers = rsaHelpers;
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
            UserTenantListModel tenantsModel = await this._userTenantContainer.GetAllAsync(tenantInput);
            List<UserTenantModel> tenantList = tenantsModel.models;

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
                // Has last used tenant and it is in the list
                if (lastUsedSetting != null && tenantList.Count(t=> t.TenantId == lastUsedSetting.Value) > 0)
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

                // Settings Update LastUsedTenant
                UserSettingsInput settingsInput = new UserSettingsInput
                {
                    userId = claims.Where(c => c.Type == "sub").First().Value,
                    settingKey = "LastUsedTenant",
                    value = tenant
                };
                // Update if name is not the same
                await this._userSettingsContainer.UpdateAsync(settingsInput);
                if (tenantModel.Name != claims.Where(c => c.Type == "name").First().Value)
                {
                    input.name = claims.Where(c => c.Type == "name").First().Value;
                    await this._userTenantContainer.UpdateAsync(input);
                }
            }

            DateTime expirationDateTime = expiration ?? DateTime.Now.AddDays(30);
            // add all tenants they have access to
            claims.AddRange(tenantList.Select(t => new Claim("available_tenants", t.TenantId)));
            
            // Token to String so you can use it in your client
            var token = this.MintToken(claims, audience, expirationDateTime);

            return token;
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
                new RsaSecurityKey(_rsaHelpers.DecodeRsa(_config.PrivateKey));

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

        public bool TryValidateToken(string audience, string encodedToken, HttpContext context, out JwtSecurityToken jwt)
        {
            jwt = null;
            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(encodedToken))
            {
                return false;
            }

            var tokenValidationParams = new TokenValidationParameters
            {
                // Validate the token signature
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = _rsaHelpers.GetJsonWebKey(this._config.PublicKey).Keys,

                // Validate the token issuer
                ValidateIssuer = false,
                ValidIssuer = _openIdProviderConfiguration.issuer,

                // Validate the token audience
                ValidateAudience = false,
                ValidAudience = audience,

                // Validate token lifetime
                ValidateLifetime = true,
                ClockSkew = new TimeSpan(0) // shouldnt be skewed as this is the same server that issued it.
            };

            SecurityToken validated_token = null;
            jwtHandler.ValidateToken(encodedToken, tokenValidationParams, out validated_token);
            if (validated_token == null)
            {
                return false;
            }

            jwt = jwtHandler.ReadJwtToken(encodedToken);
            return true;
        }
    }
}
