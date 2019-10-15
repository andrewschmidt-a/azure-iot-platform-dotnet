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
using IdentityServer4.Extensions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Net;
using IdentityGateway.Services.Exceptions;
using IdentityModel;
using RSA = IdentityGateway.Services.Helpers.RSA;
using IdentityGateway.Services.Runtime;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityGateway.Controllers
{
    [Route("")]
    public class AuthorizeController : Controller
    {
        private IServicesConfig _config;
        private IJWTHelper _jwtHelper;

        private UserTenantContainer _userTenantContainer;
        private UserSettingsContainer _userSettingsContainer;

        public AuthorizeController(IServicesConfig config, UserTenantContainer userTenantContainer,
            UserSettingsContainer userSettingsContainer, IJWTHelper jwtHelper)
        {
            this._config = config;
            this._userTenantContainer = userTenantContainer;
            this._userSettingsContainer = userSettingsContainer;
            this._jwtHelper = jwtHelper;
        }

        // GET: connect/authorize
        [HttpGet]
        [Route("connect/authorize")]
        public IActionResult Get([FromQuery] string redirect_uri, [FromQuery] string state,
            [FromQuery(Name = "client_id")] string clientId, [FromQuery] string nonce, [FromQuery] string tenant, [FromQuery] string invitation)
        {
            // Validate Input
            if (!Uri.IsWellFormedUriString(redirect_uri, UriKind.Absolute))
            {
                throw new Exception("Redirect Uri is not valid!");
            }

            Guid validatedGuid = Guid.Empty;
            if (!tenant.IsNullOrEmpty() && !Guid.TryParse(tenant, out validatedGuid))
            {
                throw new Exception("Tenant is not valid!");
            }

            var config = new Configuration(HttpContext);
            var uri = new UriBuilder(this._config.AzureB2CBaseUri);

            // Need to build Query carefully to not clobber other query items -- just injecting state
            var query = HttpUtility.ParseQueryString(uri.Query);
            query["state"] = JsonConvert.SerializeObject(new AuthState
            { returnUrl = redirect_uri, state = state, tenant = tenant, nonce = nonce, client_id = clientId, invitation = invitation });
            query["redirect_uri"] = config.issuer + "/connect/callback"; // must be https for B2C
            uri.Query = query.ToString();
            return Redirect(uri.Uri.ToString());
        }

        // GET: connect/authorize
        /// <summary>
        /// This is a pass-through auth gateway so there is no need to officially end session.
        /// Session state is never saved. Therefore, simply return to redirect. 
        /// </summary>
        /// <param name="post_logout_redirect_uri"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [Route("connect/logout")]
        public IActionResult Get([FromQuery] string post_logout_redirect_uri)
        {
            // Validate Input
            if (!Uri.IsWellFormedUriString(post_logout_redirect_uri, UriKind.Absolute))
            {
                throw new Exception("Redirect Uri is not valid!");
            }

            var uri = new UriBuilder(post_logout_redirect_uri);

            return Redirect(
                uri.Uri.ToString()
            );
        }

        // GET connect/callback
        [HttpPost("connect/switch/{tenant}")]
        public async Task<ActionResult> PostAsync([FromHeader(Name = "Authorization")] string authHeader,
            [FromRoute] string tenant)
        {
            if (authHeader == null || !authHeader.StartsWith("Bearer"))
            {
                throw new NoAuthorizationException("No Bearer Token Authorization Header was passed.");
            }

            //Extract Bearer token
            string encodedToken = authHeader.Substring("Bearer ".Length).Trim();

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = ReadToken("IoTPlatform", encodedToken);

            if (jwt.Claims.Count(c => c.Type == "available_tenants" && c.Value == tenant) > 0)
            {
                // Everything checks out so you can mint a new token
                var tokenString = jwtHandler.WriteToken(await this._jwtHelper.GetIdentityToken(jwt.Claims.Where(c => new List<string>(){"sub", "name", "email"}.Contains(c.Type)).ToList(), tenant,jwt.Audiences.First(), jwt.ValidTo));

                return StatusCode(200, tokenString);
            }
            else
            {
                throw new NoAuthorizationException("Not allowed access to this tenant.");
            }
        }

        // GET connect/callback
        [HttpPost("connect/callback")]
        public async Task<IActionResult> PostAsync([FromForm] string state, [FromForm] string id_token,
            [FromForm] string error, [FromForm] string error_description)
        {
            if (!String.IsNullOrEmpty(error))
            {
                // If there was an error returned from B2C, throw it as an expcetion
                throw new Exception($"Azure B2C returned an error: {{{error}: {error_description}}}");
            }

            AuthState authState = null;
            try
            {
                authState = JsonConvert.DeserializeObject<AuthState>(state);
            }
            catch (Exception e)
            {
                throw new Exception("Invlid state from authentication redirect", e);
            }
            
            var originalAudience = authState.client_id;

            // Bring over Subject and Name
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(id_token);
            var claims = jwt.Claims.Where(t => new List<string> { "sub", "name" }.Contains(t.Type)).ToList();

            // If theres an invitation token then add user to tenant
            if (!String.IsNullOrEmpty(authState.invitation))
            {
                var inviteJWT = jwtHandler.ReadJwtToken(authState.invitation);
                UserTenantInput UserTenant = new UserTenantInput()
                {
                    userId = claims.Where(c => c.Type == "sub").First().Value,
                    tenant = inviteJWT.Claims.Where(c => c.Type == "tenant").First().Value,
<<<<<<< HEAD
                    roles = String.Join(",", inviteJWT.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToArray())
                };
                await this._userTenantContainer.UpdateAsync(UserTenant);
            }

=======
                    roles = JsonConvert.SerializeObject(inviteJWT.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList()),
                    type = "Member"
                };
                await this._userTenantContainer.UpdateAsync(UserTenant);

                // Delete placeholder for invite
                UserTenant.userId = inviteJWT.Claims.Where(c => c.Type == "userId").First().Value;
                await this._userTenantContainer.DeleteAsync(UserTenant);
            }

>>>>>>> master
            // Extract first email
            var emailClaim = jwt.Claims.Where(t => t.Type == "emails").FirstOrDefault();
            if (emailClaim != null)
            {
                claims.Add(new Claim("email", emailClaim.Value));
            }

            if (!String.IsNullOrEmpty(authState.nonce))
            {
                claims.Add(new Claim("nonce", authState.nonce));
            }

            string tokenString = jwtHandler.WriteToken(await this._jwtHelper.GetIdentityToken(claims, authState.tenant, originalAudience, null));

            //Build Return Uri
            var returnUri = new UriBuilder(authState.returnUrl);

            // Need to build Query carefully to not clobber other query items -- just injecting state
            //var query = HttpUtility.ParseQueryString(returnUri.Query);
            //query["state"] = HttpUtility.UrlEncode(authState.state);
            //returnUri.Query = query.ToString();

            returnUri.Fragment =
                "id_token=" + tokenString + "&state=" +
                HttpUtility.UrlEncode(authState
                    .state); // pass token in Fragment for more security (Browser wont forward...)
            return Redirect(returnUri.Uri.ToString());
        }
        private JwtSecurityToken ReadToken(string audience, string encodedToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(encodedToken))
            {
                throw new NoAuthorizationException("The given token could not be read.");
            }

            var jwt = jwtHandler.ReadJwtToken(encodedToken);
            var config = new Configuration(HttpContext);

            var tokenValidationParams = new TokenValidationParameters
            {
                // Validate the token signature
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = RSA.GetJsonWebKey(this._config.PublicKey).Keys,

                // Validate the token issuer
                ValidateIssuer = false,
                ValidIssuer = config.issuer,

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
                throw new NoAuthorizationException("The given token could not be validated.");
            }
            return jwt;
        }
    }
}
public class ErrorModel
{
    public string ErrorMessage { get; set; }
}