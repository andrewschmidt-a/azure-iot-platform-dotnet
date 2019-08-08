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
    public class UserSettingsController : ControllerBase
    {

        private IConfiguration _config;
        private UserTenantTable _table;

        public KeyVaultHelper keyVaultHelper;

        public UserSettingsController(IConfiguration config, UserTenantTable table)
        {
            this._config = config;
            this._table = table;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
        }
        
        [HttpGet]
        public string Get()
        {
            return  "/api/user requires an id" ;
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<string> GetAsync(string userId, string setting)
        {
        }

        [HttpPost("{userId}")]
        public async Task<string> PostAsync(string userId, string setting, string value)
        {
        }

        [HttpPut("{userId}")]
        public void Put(string userId, string setting, string value)
        {
        }

        [HttpDelete("{userId}")]
        public async Task<string> Delete(string userId, string setting)
        {
        }
    }
}
