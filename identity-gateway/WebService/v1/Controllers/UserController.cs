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
    public class UserController : ControllerBase
    {

        private IConfiguration _config;
        private UserTenantTable _table;

        public KeyVaultHelper keyVaultHelper;

        public UserController(IConfiguration config, UserTenantTable table)
        {
            this._config = config;
            this._table = table;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
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
        public async Task<string> GetAsync(Guid id)
        {
            List <UserModel> TheModel = await this._table.GetUserTenantsAsync(id.ToString());
            return JsonConvert.SerializeObject(TheModel);
        }

        /// <summary>
        /// Create a User in table storage associated with the tenant in the header
        /// </summary>
        /// <param name="value"></param>
        // POST: api/User
        [HttpPost("{userId}")]
        public async Task<string> PostAsync(string userId, [FromBody] string roles)
        {
            var result = await this._table.CreateUserTenantAsync(userId, roles);
            return JsonConvert.SerializeObject(result);
        }

        // The way that this user table is structured leads me to thinking that there is no reason to have a put (update) api
        // this is because the table does not consist of any unique identifier, and simply serves the purpose of recording
        // which tenant id a user is associated with, so there really only needs to be create and delete apis...
        [HttpPut("{userId}")]
        public void Put(string userId, [FromBody] UserModel update)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{userId}")]
        public async Task<string> Delete(string userId)
        {
            var result = await this._table.DeleteUserTenantAsync(userId);
            return JsonConvert.SerializeObject(result);
        }
    }
}
