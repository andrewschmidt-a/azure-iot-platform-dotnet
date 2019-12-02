using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.AuthUtils;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.IdentityGateway.Services;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.v1.Controllers
{
    [Route("v1/tenants"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class UserTenantController : ControllerBase
    {
        private UserTenantContainer _container;
        private IJwtHelpers _jwtHelper;
        private readonly ISendGridClientFactory _sendGridClientFactory;

        public UserTenantController(UserTenantContainer container, IJwtHelpers jwtHelper, ISendGridClientFactory sendGridClientFactory)
        {
            this._container = container;
            this._jwtHelper = jwtHelper;
            this._sendGridClientFactory = sendGridClientFactory;
        }

        private string ClaimsUserId
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

        private string TenantId
        {
            get
            {
                try
                {
                    string tenantId = HttpContext.Request.GetTenant();
                    if (String.IsNullOrEmpty(tenantId))
                    {
                        throw new Exception("The TenantId was not attached in the user claims or request headers.");
                    }
                    return tenantId;
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to get the tenantId.", e);
                }
            }
        }

        /// <summary>
        /// Get all users for the current tenant 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("users")]
        [Authorize("ReadAll")]
        public async Task<UserTenantListModel> GetAllUsersForTenantAsync()
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = null,
                Tenant = this.TenantId
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
            return await this.GetAllTenantsForUserAsync(this.ClaimsUserId);
        }

        /// <summary>
        /// Get all tenants for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}/all")]
        [Authorize("ReadAll")]
        public async Task<UserTenantListModel> GetAllTenantsForUserAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = userId,
            };
            return await this._container.GetAllAsync(input);
        }

        /// <summary>
        /// Get the User-Tenant realtionship model by using the userId from the claims
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize("ReadAll")]
        public async Task<UserTenantModel> UserClaimsGetAsync()
        {
            return await this.GetAsync(this.ClaimsUserId);
        }

        /// <summary>
        /// Get User-Tenant relationship model by Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        // GET: api/User/5
        [HttpGet("{userId}")]
        [Authorize("ReadAll")]
        public async Task<UserTenantModel> GetAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = userId,
                Tenant = this.TenantId
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
            return await this.PostAsync(this.ClaimsUserId, model);
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
                UserId = userId,
                Tenant = this.TenantId,
                Roles = model.Roles
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
            return await this.PutAsync(this.ClaimsUserId, update);
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
                UserId = userId,
                Tenant = this.TenantId,
                Roles = update.Roles
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
            return await this.DeleteAsync(this.ClaimsUserId);
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
                UserId = userId,
                Tenant = this.TenantId
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
                Tenant = this.TenantId
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
                UserId = Guid.NewGuid().ToString(),
                Tenant = this.TenantId,
                Roles = JsonConvert.SerializeObject(new List<string>() { invitation.role }),
                Name = invitation.email_address,
                Type = "Invited"
            };

            List<Claim> claims = new List<Claim>()
            {
                new Claim("role", invitation.role),
                new Claim("tenant", this.TenantId),
                new Claim("userId", input.UserId)
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
            msg.AddContent(MimeType.Html, "<a href=\"" + link + "\">" + link + "</a>");

            var client = _sendGridClientFactory.CreateSendGridClient();
            var response = await client.SendEmailAsync(msg);

            return await this._container.CreateAsync(input);
        }
    }
}
