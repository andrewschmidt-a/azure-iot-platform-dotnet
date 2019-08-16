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
        }

        [HttpGet("{userId}/{setting}")]
        public async Task<string> GetAsync(string userId, string setting)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                userId = userId,
                settingKey = setting
            };
            UserSettingsModel model = await this._table.GetAsync(input);
            return JsonConvert.SerializeObject(model);
        }

        [HttpPost("{userId}/{setting}/{value}")]
        public async Task<string> PostAsync(string userId, string setting, string value)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                userId = userId,
                settingKey = setting,
                value = value
            };
            UserSettingsModel model = await this._table.PostAsync(input);
            return JsonConvert.SerializeObject(model);
        }

        [HttpPut("{userId}/{setting}/{value}")]
        public async Task<string> PutAsync(string userId, string setting, string value)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                userId = userId,
                settingKey = setting,
                value = value
            };
            UserSettingsModel model = await this._table.PutAsync(input);
            return JsonConvert.SerializeObject(model);
        }

        [HttpDelete("{userId}/{setting}")]
        public async Task<string> DeleteAsync(string userId, string setting)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                userId = userId,
                settingKey = setting,
            };
            UserSettingsModel model = await this._table.DeleteAsync(input);
            return JsonConvert.SerializeObject(model);
        }
    }
}
