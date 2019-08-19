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
    [Route("v1/settings")]
    public class UserSettingsController : ControllerBase
    {

        private IConfiguration _config;
        private UserSettingsContainer _container;

        public KeyVaultHelper keyVaultHelper;

        public UserSettingsController(IConfiguration config, UserSettingsContainer container)
        {
            this._config = config;
            this._container = container;
            this.keyVaultHelper = new KeyVaultHelper(this._config);
        }

        [HttpGet("{userId}")]
        public async Task<string> GetAllAsync(string userId)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                userId = userId,
            };
            List<UserSettingsModel> models = await this._container.GetAllAsync(input);
            return JsonConvert.SerializeObject(models);
        }
        
        [HttpGet("{userId}/{setting}")]
        public async Task<string> GetAsync(string userId, string setting)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                userId = userId,
                settingKey = setting
            };
            UserSettingsModel model = await this._container.GetAsync(input);
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
            UserSettingsModel model = await this._container.CreateAsync(input);
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
            UserSettingsModel model = await this._container.UpdateAsync(input);
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
            UserSettingsModel model = await this._container.DeleteAsync(input);
            return JsonConvert.SerializeObject(model);
        }
    }
}
