// <copyright file="UserTenantController.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.IdentityGateway.Services;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.Controllers
{
    [Route("v1/tenants")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class UserTenantController : Controller
    {
        private readonly ISendGridClientFactory sendGridClientFactory;
        private UserTenantContainer container;
        private IJwtHelpers jwtHelper;

        public UserTenantController(UserTenantContainer container, IJwtHelpers jwtHelper, ISendGridClientFactory sendGridClientFactory)
        {
            this.container = container;
            this.jwtHelper = jwtHelper;
            this.sendGridClientFactory = sendGridClientFactory;
        }

        [HttpGet("users")]
        [Authorize("ReadAll")]
        public async Task<UserTenantListModel> GetAllUsersForTenantAsync()
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = null,
                Tenant = this.GetTenantId(),
            };
            return await this.container.GetAllUsersAsync(input);
        }

        [HttpGet("all")]
        public async Task<UserTenantListModel> UserClaimsGetAllTenantsForUserAsync()
        {
            return await this.GetAllTenantsForUserAsync(this.GetClaimsUserId());
        }

        [HttpGet("{userId}/all")]
        [Authorize("ReadAll")]
        public async Task<UserTenantListModel> GetAllTenantsForUserAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = userId,
            };
            return await this.container.GetAllAsync(input);
        }

        [HttpGet("")]
        [Authorize("ReadAll")]
        public async Task<UserTenantModel> UserClaimsGetAsync()
        {
            return await this.GetAsync(this.GetClaimsUserId());
        }

        [HttpGet("{userId}")]
        [Authorize("ReadAll")]
        public async Task<UserTenantModel> GetAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = userId,
                Tenant = this.GetTenantId(),
            };
            return await this.container.GetAsync(input);
        }

        [HttpPost("")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> UserClaimsPostAsync([FromBody] UserTenantModel model)
        {
            return await this.PostAsync(this.GetClaimsUserId(), model);
        }

        [HttpPost("{userId}")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> PostAsync(string userId, [FromBody] UserTenantModel model)
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = userId,
                Tenant = this.GetTenantId(),
                Roles = model.Roles,
                Name = model.Name,
                Type = model.Type,
            };
            return await this.container.CreateAsync(input);
        }

        [HttpPut("")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> UserClaimsPutAsync([FromBody] UserTenantModel update)
        {
            return await this.PutAsync(this.GetClaimsUserId(), update);
        }

        [HttpPut("{userId}")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> PutAsync(string userId, [FromBody] UserTenantModel update)
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = userId,
                Tenant = this.GetTenantId(),
                Roles = update.Roles,
            };
            return await this.container.UpdateAsync(input);
        }

        [HttpDelete("")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> UserClaimsDeleteAsync()
        {
            return await this.DeleteAsync(this.GetClaimsUserId());
        }

        [HttpDelete("{userId}")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> DeleteAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                UserId = userId,
                Tenant = this.GetTenantId(),
            };
            return await this.container.DeleteAsync(input);
        }

        [HttpDelete("all")]
        [Authorize("UserManage")]
        public async Task<UserTenantListModel> DeleteAllAsync()
        {
            UserTenantInput input = new UserTenantInput
            {
                Tenant = this.GetTenantId(),
            };
            return await this.container.DeleteAllAsync(input);
        }

        [HttpPost("invite")]
        [Authorize("UserManage")]
        public async Task<UserTenantModel> InviteAsync([FromBody] Invitation invitation)
        {
            // Object to insert in table as placeholder
            UserTenantInput input = new UserTenantInput
            {
                UserId = Guid.NewGuid().ToString(),
                Tenant = this.GetTenantId(),
                Roles = JsonConvert.SerializeObject(new List<string>() { invitation.Role }),
                Name = invitation.EmailAddress,
                Type = "Invited",
            };

            List<Claim> claims = new List<Claim>()
            {
                new Claim("role", invitation.Role),
                new Claim("tenant", this.GetTenantId()),
                new Claim("userId", input.UserId),
            };

            string forwardedFor = null;

            // add issuer with forwarded for address if exists (added by reverse proxy)
            if (HttpContext.Request.Headers.Where(t => t.Key == "X-Forwarded-For").Count() > 0)
            {
                forwardedFor = HttpContext.Request.Headers.Where(t => t.Key == "X-Forwarded-For").FirstOrDefault().Value
                    .First();
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            string inviteToken = jwtHandler.WriteToken(this.jwtHelper.MintToken(claims, "IdentityGateway", DateTime.Now.AddDays(3)));

            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress("iotplatformnoreply@mmm.com", "3M IoT Platform Team"));

            var recipients = new List<EmailAddress>
            {
                new EmailAddress(invitation.EmailAddress),
            };
            msg.AddTos(recipients);

            msg.SetSubject("Invitation to IoT Platform");
            Uri uri = new Uri(forwardedFor ?? "https://" + HttpContext.Request.Host.ToString());
            string link = uri.Host + "#invite=" + inviteToken;
            msg.AddContent(MimeType.Text, "Click here to join the tenant: ");
            msg.AddContent(MimeType.Html, "<a href=\"" + link + "\">" + link + "</a>");

            var client = sendGridClientFactory.CreateSendGridClient();
            var response = await client.SendEmailAsync(msg);

            return await this.container.CreateAsync(input);
        }
    }
}