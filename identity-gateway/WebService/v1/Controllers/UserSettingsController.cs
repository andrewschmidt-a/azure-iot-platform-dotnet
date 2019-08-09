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
        private UserSettingsContainer _table;

        public KeyVaultHelper keyVaultHelper;

        public UserSettingsController(IConfiguration config, UserSettingsContainer table)
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

        [HttpGet("{userId}/{setting}")]
        public async Task<string> GetAsync(string userId, string setting)
        {
            return "get";
        }

        [HttpPost("{userId}/{setting}/{value}")]
        public async Task<string> PostAsync(string userId, [FromBody] UserSettingsModel model)
        {
            return "post";
        }

        [HttpPut("{userId}/{setting}/{value}")]
        public async Task<string> PutAsync(string userId, [FromBody] UserSettingsModel model)
        {
            return "put";
        }

        [HttpDelete("{userId}/{setting}")]
        public async Task<string> Delete(string userId, string setting)
        {
            return "delete";
        }
    }
}
