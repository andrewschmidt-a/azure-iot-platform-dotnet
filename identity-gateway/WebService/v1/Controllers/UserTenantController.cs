using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityGateway.Services;
using IdentityGateway.Services.Models;
using IdentityGateway.WebService.v1.Filters;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Authorization;

namespace IdentityGateway.WebService.v1.Controllers
{
    [Route("v1/tenants"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class UserTenantController : ControllerBase
    {
        private const string ADMIN_ROLE = "admin";
        private const string ADMIN_ROLE = "readonly";
        private IConfiguration _config;
        private UserTenantContainer _container;

        public UserTenantController(IConfiguration config, UserTenantContainer container)
        {
            this._config = config;
            this._container = container;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/User/5
        [HttpGet("{userId}")]
        [Authorize(Roles = "readonly,admin")]
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
        [Authorize(Roles = "admin")]
        public async Task<string> PostAsync(string userId, [FromBody] UserTenantModel model)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = ,
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
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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
        [HttpGet("invite")]
        [Authorize(Roles = "admin")]
        public async Task<string> InviteAsync([FromBody] string emailAddress, [FromBody] string role)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = new Guid().ToString(),
                tenant = this._container.tenant
            };
            UserTenantModel model = await this._container.GetAsync(input);
            return JsonConvert.SerializeObject(model);
        }
    }
}
