// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Config.Services;
using Mmm.Platform.IoT.Common.WebService.v1;
using Mmm.Platform.IoT.Common.WebService.v1.Filters;

namespace Mmm.Platform.IoT.Config.WebService.v1.Controllers
{
    [Route(Version.PATH), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class UserSettingsController : Controller
    {
        private readonly IStorage storage;

        public UserSettingsController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet("user-settings/{id}")]
        [Authorize("ReadAll")]
        public async Task<object> GetUserSettingAsync(string id)
        {
            return await this.storage.GetUserSetting(id);
        }

        [HttpPut("user-settings/{id}")]
        [Authorize("ReadAll")]
        public async Task<object> SetUserSettingAsync(string id, [FromBody] object setting)
        {
            return await this.storage.SetUserSetting(id, setting);
        }
    }
}
