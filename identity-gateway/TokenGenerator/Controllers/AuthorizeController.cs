using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using tenant_manager.Helpers;
using tenant_manager.Models;
using System.Security.Claims;
using System.Web;
using TokenGenerator.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault;
using TokenGenerator.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TokenGenerator.Controllers
{

    public class AuthorizeController : Controller
    {
        private IConfiguration _config;
        public KeyVaultHelper keyVaultHelper; //used for Testing
        public TenantTableHelper tenantTableHelper; //used for Testing
        public AuthorizeController(IConfiguration config)
        {
            this._config = config;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
            this.tenantTableHelper = new TenantTableHelper();
        }
        // GET: connect/authorize
        [HttpGet]
        [Route("connect/authorize")]
        public IActionResult Get([FromQuery] string  returnUrl, [FromQuery] string state, [FromQuery] string tenant)
        {
            var uri = new UriBuilder(this._config["AzureB2CBaseUri"]);

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
        [HttpPost]
        [Route("connect/callback")]
        public async Task<IActionResult> PostAsync([FromForm] string state, [FromForm] string id_token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            if (jwtHandler.CanReadToken(id_token))
            {
                string tenantStorageAccountConnectionString = "";
                string identityGatewayPrivateKey = "";
                try
                {
                    /* Get Secrets From KeyVault */

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

                TenantModel tenantModel = await tenantTableHelper.GetUserTenantInfo(tenantStorageAccountConnectionString, authState.tenant, jwt.Claims.First(t => t.Type == "sub").Value);

                // If User not associated with Tenant then dont add claims return token without 
                if (tenantModel != null)
                {
                    // Add Tenant
                    claims.Add(new Claim("tenant", tenantModel.RowKey));
                    // Add Roles
                    tenantModel.RoleList.ForEach(role => claims.Add(new Claim("role", role)));
                }

                // Create Security key  using private key above:
                // not that latest version of JWT using Microsoft namespace instead of System
                var securityKey = new RsaSecurityKey(TokenGenerator.Helpers.Encryption.RSA.DecodeRSA(identityGatewayPrivateKey));
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

            return View();
        }

    }
}
