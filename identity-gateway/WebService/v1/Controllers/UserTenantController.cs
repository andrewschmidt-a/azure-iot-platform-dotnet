using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityGateway.Services;
using IdentityGateway.Services.Models;
using IdentityGateway.Services.Helpers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace IdentityGateway.WebService.Controllers
{
    [Route("v1/tenants")]
    public class UserTenantController : ControllerBase
    {

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
        /// 
        /// </summary>
        /// <param name="id"></param>
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{userId}")]
        public async Task<string> Delete(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                tenant = this._container.tenant
            };
            var result = await this._container.DeleteAsync(input);
            return JsonConvert.SerializeObject(result);
        }
    }
}
