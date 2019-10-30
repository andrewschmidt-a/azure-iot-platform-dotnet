using System;
using System.Threading.Tasks;
using IdentityGateway.AuthUtils;
using IdentityGateway.Services;
using IdentityGateway.Services.Models;
using IdentityGateway.WebService.v1.Filters;
using Microsoft.AspNetCore.Mvc;

namespace IdentityGateway.WebService.v1.Controllers
{
    [Route("v1/settings"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    [Authorize("ReadAll")]
    public class UserSettingsController : ControllerBase
    {
        private UserSettingsContainer _container;

        public UserSettingsController(UserSettingsContainer container)
        {
            _container = container;
        }

        private string ClaimsUserId
        {
            get
            {
                try
                {
                    return HttpContext.Request.GetCurrentUserObjectId();
                }
                catch (Exception e)
                {
                    throw new Exception("A request was sent to an API endpoint that requires a userId, but the userId was not passed through the url nor was it available in the user Claims.", e);
                }
            }
        }

        /// <summary>
        /// Get all settings for the user id from the claims
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<UserSettingsListModel> UserClaimsGetAllAsync()
        {
            return await this.GetAllAsync(this.ClaimsUserId);
        }

        /// <summary>
        /// get all settings for the given userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}/all")]
        public async Task<UserSettingsListModel> GetAllAsync(string userId)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                UserId = userId,
            };
            return await this._container.GetAllAsync(input);
        }

        /// <summary>
        /// Get the setting of the given key for the userId in the claims
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        [HttpGet("{setting}")]
        public async Task<UserSettingsModel> UserClaimsGetAsync(string setting)
        {
            return await this.GetAsync(this.ClaimsUserId, setting);
        }
        
        /// <summary>
        /// Get the setting of the given key for the given userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        [HttpGet("{userId}/{setting}")]
        public async Task<UserSettingsModel> GetAsync(string userId, string setting)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                UserId = userId,
                SettingKey = setting
            };
            return await this._container.GetAsync(input);
        }

        /// <summary>
        /// Set the setting of the given key, to the given value, for the userId in the claims
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("{setting}/{value}")]
        [Authorize("UserManage")]
        public async Task<UserSettingsModel> UserClaimsPostAsync(string setting, string value)
        {
            return await this.PostAsync(this.ClaimsUserId, setting, value);
        }

        /// <summary>
        /// Set the setting of the given key, to the given value, for the given userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("{userId}/{setting}/{value}")]
        [Authorize("UserManage")]
        public async Task<UserSettingsModel> PostAsync(string userId, string setting, string value)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                UserId = userId,
                SettingKey = setting,
                Value = value
            };
            return await this._container.CreateAsync(input);
        }

        /// <summary>
        /// Update the setting of the given key, to the given value, for the userId in the claims
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("{setting}/{value}")]
        [Authorize("UserManage")]
        public async Task<UserSettingsModel> UserClaimsPutAsync(string setting, string value)
        {
            return await this.PutAsync(this.ClaimsUserId, setting, value);
        }

        /// <summary>
        /// Update the setting of the given key, to the given value, for the given userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("{userId}/{setting}/{value}")]
        [Authorize("UserManage")]
        public async Task<UserSettingsModel> PutAsync(string userId, string setting, string value)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                UserId = userId,
                SettingKey = setting,
                Value = value
            };
            return await this._container.UpdateAsync(input);
        }

        /// <summary>
        /// Delete the setting of the given key, for the userId in the claims
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        [HttpDelete("{setting}")]
        [Authorize("UserManage")]
        public async Task<UserSettingsModel> UserClaimsDeleteAsync(string setting)
        {
            return await this.DeleteAsync(this.ClaimsUserId, setting);
        }

        /// <summary>
        /// Delete the setting of the given key, for the given userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        [HttpDelete("{userId}/{setting}")]
        [Authorize("UserManage")]
        public async Task<UserSettingsModel> DeleteAsync(string userId, string setting)
        {
            UserSettingsInput input = new UserSettingsInput
            {
                UserId = userId,
                SettingKey = setting,
            };
            return await this._container.DeleteAsync(input);
        }
    }
}
