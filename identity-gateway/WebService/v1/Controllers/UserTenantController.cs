using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityGateway.Services;
using IdentityGateway.Services.Models;
using IdentityGateway.WebService.v1.Filters;
using IdentityGateway.AuthUtils;
using Newtonsoft.Json;
using System;
using IdentityGateway.Services.Runtime;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using IdentityGateway.Services.Helpers;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Linq;
using WebService;

namespace IdentityGateway.WebService.v1.Controllers
{
    [Route("v1/tenants"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    [Authorize("ReadAll")]
    public class UserTenantController : ControllerBase
    {
        private IServicesConfig _config;
        private UserTenantContainer _container;
        private IJwtHelpers _jwtHelper;
        private readonly ISendGridClientFactory _sendGridClientFactory;

        public UserTenantController(IServicesConfig config, UserTenantContainer container, IJwtHelpers jwtHelper, ISendGridClientFactory sendGridClientFactory)
        {
            this._config = config;
            this._container = container;
            this._jwtHelper = jwtHelper;
            this._sendGridClientFactory = sendGridClientFactory;
        }

        private string claimsUserId
        {
            get
            {
                try
                {
                    return HttpContext.Request.GetCurrentUserObjectId();
                }
                catch (Exception e)
                {
                    throw new Exception("A request was sent to an API endpoint that requires a userId, but the userId was not passed through the url nor was it available in the user Claims.", e);
                }
            }
        }

        private string tenantId
        {
            get
            {
                try
                {
                    return HttpContext.Request.GetTenant();
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to get the tenantId from the HttpContext.", e);
                }
            }
        }

        /// <summary>
        /// Get all users for the current tenant 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("users")]
        public async Task<UserTenantListModel> GetAllUsersForTenantAsync()
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = null,
                tenant = this.tenantId
            };
            return await this._container.GetAllUsersAsync(input);
        }

        /// <summary>
        /// Get all tenants for a user using the userId in the claims
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<UserTenantListModel> UserClaimsGetAllTenantsForUserAsync()
        {
            return await this.GetAllTenantsForUserAsync(this.claimsUserId);
        }
        
        /// <summary>
        /// Get all tenants for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}/all")]
        public async Task<UserTenantListModel> GetAllTenantsForUserAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
            };
            return await this._container.GetAllAsync(input);
        }

        /// <summary>
        /// Get the User-Tenant realtionship model by using the userId from the claims
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<UserTenantModel> UserClaimsGetAsync()
        {
            return await this.GetAsync(this.claimsUserId);
        }

        /// <summary>
        /// Get User-Tenant relationship model by Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        // GET: api/User/5
        [HttpGet("{userId}")]
        public async Task<UserTenantModel> GetAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this.tenantId
            };
            return await this._container.GetAsync(input);
        }

        /// <summary>
        /// Create a user-tenant relationship record using the userId from the claims
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> UserClaimsPostAsync([FromBody] UserTenantModel model)
        {
            return await this.PostAsync(this.claimsUserId, model);
        }

        /// <summary>
        /// Create a User in container storage associated with the tenant in the header
        /// </summary>
        /// <param name="value"></param>
        [HttpPost("{userId}")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> PostAsync(string userId, [FromBody] UserTenantModel model)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this.tenantId,
                roles = model.Roles,
            };
            return await this._container.CreateAsync(input);
        }

        /// <summary>
        /// Update a user-tenant relationship record using the userId from the claims
        /// </summary>
        /// <param name="update"></param>
        [HttpPut("")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> UserClaimsPutAsync([FromBody] UserTenantModel update)
        {
            return await this.PutAsync(this.claimsUserId, update);
        }

        /// <summary>
        /// Update a user-tenant relationship record
        /// </summary>
        /// <param name="update"></param>
        [HttpPut("{userId}")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> PutAsync(string userId, [FromBody] UserTenantModel update)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this.tenantId,
                roles = update.Roles,
            };
            return await this._container.UpdateAsync(input);
        }

        /// <summary>
        /// Delete a user-tenant relationship record using the userId from the claims
        /// </summary>
        /// <param name="userId"></param>
        [HttpDelete("")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> UserClaimsDeleteAsync()
        {
            return await this.DeleteAsync(this.claimsUserId);
        }

        /// <summary>
        /// Delete a user-tenant relationship record
        /// </summary>
        /// <param name="userId"></param>
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{userId}")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> DeleteAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this.tenantId
            };
            return await this._container.DeleteAsync(input);
        }

        /// <summary>
        /// Delete the tenant from all users
        /// </summary>
        [HttpDelete("all")]
        [Authorize("UserManage")]
        public async Task<UserTenantListModel> DeleteAllAsync()
        {
            UserTenantInput input = new UserTenantInput
            {
                tenant = this.tenantId
            };
            return await this._container.DeleteAllAsync(input);
        }
        /// <summary>
        /// Invite the user to join a tenant
        /// </summary>
        /// <returns></returns>
        // GET: api/User/5
        [HttpPost("invite")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> InviteAsync([FromBody] Invitation invitation)
        {
            // Object to insert in table as placeholder
            UserTenantInput input = new UserTenantInput
            {
                userId = Guid.NewGuid().ToString(),
                tenant = this.tenantId,
                roles = JsonConvert.SerializeObject(new List<string>() { invitation.role }),
                name = invitation.email_address,
                type = "Invited"
            };

            List<Claim> claims = new List<Claim>()
            {
                new Claim("role", invitation.role),
                new Claim("tenant", this.tenantId),
                new Claim("userId", input.userId)
            };

            string forwardedFor = null;
            // add issuer with forwarded for address if exists (added by reverse proxy)
            if (HttpContext.Request.Headers.Where(t => t.Key == "X-Forwarded-For").Count() > 0)
            {
                forwardedFor = HttpContext.Request.Headers.Where(t => t.Key == "X-Forwarded-For").FirstOrDefault().Value
                    .First();
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            string inviteToken = jwtHandler.WriteToken(this._jwtHelper.MintToken(claims, "IdentityGateway", DateTime.Now.AddDays(3)));

            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress("iotplatformnoreply@mmm.com", "3M IoT Platform Team"));

            var recipients = new List<EmailAddress>
            {
                new EmailAddress(invitation.email_address)
            };
            msg.AddTos(recipients);

            msg.SetSubject("Invitation to IoT Platform");
            Uri uri = new Uri(forwardedFor ?? "https://" + HttpContext.Request.Host.ToString());
            string link = uri.Host + "#invite=" + inviteToken;
            msg.AddContent(MimeType.Text, "Click here to join the tenant: ");
            msg.AddContent(MimeType.Html, "<a href=\""+ link + "\">"+link+"</a>");

            var client = _sendGridClientFactory.CreateSendGridClient();
            var response = await client.SendEmailAsync(msg);

            return await this._container.CreateAsync(input);
        }
    }
}
