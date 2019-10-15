using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityGateway.Services;
using IdentityGateway.Services.Models;
using IdentityGateway.WebService.v1.Filters;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System;
using RSA = IdentityGateway.Services.Helpers.RSA;
using Microsoft.IdentityModel.Tokens;
using IdentityGateway.Services.Runtime;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using IdentityGateway.Services.Helpers;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Linq;

namespace IdentityGateway.WebService.v1.Controllers
{
    [Route("v1/tenants"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    [Authorize("ReadAll")]
    public class UserTenantController : ControllerBase
    {
        private IServicesConfig _config;
        private UserTenantContainer _container;
        private IJWTHelper _jwtHelper;

        public UserTenantController(IServicesConfig config, UserTenantContainer container, IJWTHelper jwtHelper)
        {
            this._config = config;
            this._container = container;
            this._jwtHelper = jwtHelper;
        }

        /// <summary>
        /// Get all users in tenant
        /// </summary>
        /// <returns></returns>
        [HttpGet("users")]
        public async Task<List<UserTenantModel>> GetAllAsync()
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = null,
                tenant = this._container.tenant
            };
            List<UserTenantModel> models = await this._container.GetAllUsersAsync(input);
            return models;
        }

        /// <summary>
        /// Get User by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/User/5
        [HttpGet("{userId}")]
        public async Task<string> GetAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this._container.tenant
            };
            UserTenantModel model = await this._container.GetAsync(input);
            return JsonConvert.SerializeObject(model);
        }

        /// <summary>
        /// Create a User in container storage associated with the tenant in the header
        /// </summary>
        /// <param name="value"></param>
        [HttpPost("{userId}")]
        [Authorize("UserManage")]
        public async Task<string> PostAsync(string userId, [FromBody] UserTenantModel model)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this._container.tenant,
                roles = model.Roles,
            };
            var result = await this._container.CreateAsync(input);
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="update"></param>
        [HttpPut("{userId}")]
        [Authorize("UserManage")]
        public async Task<string> PutAsync(string userId, [FromBody] UserTenantModel update)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this._container.tenant,
                roles = update.Roles,
            };
            var result = await this._container.UpdateAsync(input);
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Delete the tenant from a user
        /// </summary>
        /// <param name="id"></param>
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{userId}")]
        [Authorize("UserManage")]
        public async Task<string> DeleteAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this._container.tenant
            };
            var result = await this._container.DeleteAsync(input);
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Delete the tenant from all users
        /// </summary>
        [HttpDelete("")]
        [Authorize("UserManage")]
        public async Task<string> DeleteAllAsync()
        {
            var result = await this._container.DeleteAllAsync();
            return JsonConvert.SerializeObject(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
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
                tenant = this._container.tenant,
                roles = JsonConvert.SerializeObject(new List<string>() { invitation.role }),
                name = invitation.email_address,
                type = "Invited"
            };

            List<Claim> claims = new List<Claim>()
            {
                new Claim("role", invitation.role),
                new Claim("tenant", this._container.tenant),
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
<<<<<<< HEAD
                new EmailAddress(email_address)
=======
                new EmailAddress(invitation.email_address)
>>>>>>> master
            };
            msg.AddTos(recipients);

            msg.SetSubject("Invitation to IoT Platform");
<<<<<<< HEAD
            string link = forwardedFor ?? "https://" + HttpContext.Request.Host.ToString() + "#invite=" + inviteToken;
=======
            Uri uri = new Uri(forwardedFor ?? "https://" + HttpContext.Request.Host.ToString());
            string link = uri.Host + "#invite=" + inviteToken;
>>>>>>> master
            msg.AddContent(MimeType.Text, "Click here to join the tenant: ");
            msg.AddContent(MimeType.Html, "<a href=\""+ link + "\">"+link+"</a>");

            var client = new SendGridClient(this._config.SendGridAPIKey);
            var response = await client.SendEmailAsync(msg);

<<<<<<< HEAD
            return response.Body.ToString();
=======
            return await this._container.CreateAsync(input);
>>>>>>> master
        }
    }
}
