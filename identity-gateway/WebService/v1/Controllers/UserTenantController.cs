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
    [Route("api/[controller]")]
    public class UserTenantController : ControllerBase
    {

        private IConfiguration _config;
        private UserTenantContainer _table;

        public UserTenantController(IConfiguration config, UserTenantContainer table)
        {
            this._config = config;
            this._table = table;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET: api/User
        [HttpGet]
        public string Get()
        {
            return  "/api/user requires an id" ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/User/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<string> GetAsync(string userId)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId
            };
            UserTenantModel TheModel = await this._table.GetAsync(input);
            return JsonConvert.SerializeObject(TheModel);
        }

        /// <summary>
        /// Create a User in table storage associated with the tenant in the header
        /// </summary>
        /// <param name="value"></param>
        [HttpPost("{userId}")]
        public async Task<string> PostAsync(string userId, [FromBody] UserTenantModel model)
        {
            UserTenantInput input = new UserTenantInput
            {
                userId = userId,
                roles = model.roles
            };
            var result = await this._table.CreateAsync(input);
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
                roles = model.roles
            };
            var result = await this._table.UpdateAsync(input);
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
                userId = userId
            };
            var result = await this._table.DeleteAsync(input);
            return JsonConvert.SerializeObject(result);
        }
    }
}
