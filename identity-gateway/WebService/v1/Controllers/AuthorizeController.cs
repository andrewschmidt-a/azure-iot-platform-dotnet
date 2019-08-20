using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using IdentityGateway.Services;
using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityGateway.Controllers
{
    [Route("")]
    public class AuthorizeController : Controller
    {

        private IConfiguration _config;
        public UserTenantContainer _userTenantContainer;
        public UserSettingsContainer _userSettingsContainer;

        public KeyVaultHelper keyVaultHelper;

        public AuthorizeController(IConfiguration config, UserTenantContainer userTenantContainer, UserSettingsContainer userSettingsContainer)
        {
            this._config = config;
            this._userTenantContainer = userTenantContainer;
            this._userSettingsContainer = userSettingsContainer;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
        }
        // GET: connect/authorize
        [HttpGet("connect/authorize")]
        public IActionResult Get([FromQuery] string  returnUrl, [FromQuery] string state, [FromQuery(Name = "client_id")] string tenant)
        {
            var uri = new UriBuilder(this._config["Global:AzureB2CBaseUri"]);

            // Need to build Query carefully to not clobber other query items -- just injecting state
            var query = HttpUtility.ParseQueryString(uri.Query);
            query["state"] = JsonConvert.SerializeObject(new AuthState { returnUrl = returnUrl, state = state, tenant = tenant });
            query["redirect_uri"] = "https://"+HttpContext.Request.Host.ToString() + "/connect/callback"; // must be https for B2C
            uri.Query = query.ToString();
            return Redirect(
                uri.Uri.ToString()
            );
        }

        // GET connect/callback
        [HttpPost("connect/callback")]
        public async Task<IActionResult> PostAsync([FromForm] string state, [FromForm] string id_token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            if (jwtHandler.CanReadToken(id_token))
            {
                string tenantStorageAccountConnectionString = "";
                string identityGatewayPrivateKey = "";
                try
                {
                    // Get Secrets From KeyVault
                    var listOfTasks = new Task<string>[] {
                        keyVaultHelper.getSecretAsync("tenantStorageAccountConnectionString"),
                        keyVaultHelper.getSecretAsync("identityGatewayPrivateKey")
                    };
                    Task.WaitAll(listOfTasks);
                    tenantStorageAccountConnectionString = listOfTasks[0].Result;
                    identityGatewayPrivateKey = listOfTasks[1].Result;
                }
                /* If you have throttling errors see this tutorial https://docs.microsoft.com/azure/key-vault/tutorial-net-create-vault-azure-web-app */
                /// <exception cref="KeyVaultErrorException">
                /// Thrown when the operation returned an invalid status code
                /// </exception>
                catch (KeyVaultErrorException keyVaultException)
                {
                    throw keyVaultException; 
                }
                var jwt = jwtHandler.ReadJwtToken(id_token);
                var authState = JsonConvert.DeserializeObject<AuthState>(state);

                // Bring over Subject and Name
                var claims = jwt.Claims.Where(t=> new List<string> { "sub", "name" }.Contains(t.Type)).ToList();
                var userId = jwt.Claims.First(t => t.Type == "sub").Value;

                // Create a userTenantInput for the purpose of finding the full tenant list associated with this user
                UserTenantInput tenantInput = new UserTenantInput
                {
                    userId = userId
                };
                List<UserTenantModel> tenantList = await this._userTenantContainer.GetAllAsync(tenantInput);

                if (authState.tenant == null)
                {
                    // authState has no tenant, so we should use either the User's last used tenant, or the first tenant available to them
                    // Create a UserSettingsInput for the purpose of finding the LastUsedTenant setting for this user
                    UserSettingsInput settingsInput = new UserSettingsInput
                    {
                        userId = userId,
                        settingKey = "LastUsedTenant"
                    };
                    UserSettingsModel lastUsedSetting = await this._userSettingsContainer.GetAsync(settingsInput);
                    string tenant = lastUsedSetting.Value;  // Get the value of the last used tenant setting

                    if (String.IsNullOrEmpty(tenant))
                    {
                        tenant = tenantList.First().RowKey;  // Set the tenant to the first tenant in the list of tenants for this user
                    }
                    authState.tenant = tenant;
                }

                UserTenantInput input = new UserTenantInput
                {
                    userId = userId,
                    tenant = authState.tenant
                };
                UserTenantModel tenantModel = await this._userTenantContainer.GetAsync(input);

                // If User not associated with Tenant then dont add claims return token without 
                if (tenantModel != null)
                {
                    // Add Tenant
                    claims.Add(new Claim("tenant", tenantModel.RowKey));
                    // Add Roles
                    tenantModel.RoleList.ForEach(role => claims.Add(new Claim("role", role)));
                }

                // add all tenants they have access to
                claims.AddRange(tenantList.Select(t=> new Claim("available_tenants", t.RowKey)));

                // Create Security key  using private key above:
                // not that latest version of JWT using Microsoft namespace instead of System
                var securityKey = new RsaSecurityKey(IdentityGateway.Services.Helpers.RSA.DecodeRSA(identityGatewayPrivateKey));
                // Also note that securityKey length should be >256b
                // so you have to make sure that your private key has a proper length
                //
                var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials
                                  (securityKey, SecurityAlgorithms.RsaSha256);

                var token = new JwtSecurityToken(
                  issuer: "https://"+HttpContext.Request.Host.ToString()+"/",
                  audience: "IoTPlatform",
                  expires: DateTime.Now.AddDays(30),
                  claims: claims.ToArray(),                      
                  signingCredentials: credentials
                );
                // Token to String so you can use it in your client
                var tokenString = jwtHandler.WriteToken(token);

                //Build Return Uri
                var returnUri = new UriBuilder(authState.returnUrl);

                // Need to build Query carefully to not clobber other query items -- just injecting state
                var query = HttpUtility.ParseQueryString(returnUri.Query);
                query["state"] = HttpUtility.UrlEncode(authState.state);
                returnUri.Query = query.ToString();

                returnUri.Fragment = "id_token=" + tokenString; // pass token in Fragment for more security (Browser wont forward...)
                return Redirect(returnUri.Uri.ToString());
            }
            else
            {
                throw new Exception("Invalid Token!");
            }
        }
    }
}
