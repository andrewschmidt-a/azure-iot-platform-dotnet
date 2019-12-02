// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.Config.Services;
using Mmm.Platform.IoT.Config.Services.Models;
using Mmm.Platform.IoT.Config.WebService.v1.Models;

namespace Mmm.Platform.IoT.Config.WebService.v1.Controllers
{
    [Route(Version.PATH), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class SolutionSettingsController : Controller
    {
        private readonly IStorage storage;
        private readonly IActions actions;

        private static readonly string ACCESS_CONTROL_EXPOSE_HEADERS = "Access-Control-Expose-Headers";

        public SolutionSettingsController(IStorage storage, IActions actions)
        {
            this.storage = storage;
            this.actions = actions;
        }

        [HttpGet("solution-settings/theme")]
        [Authorize("ReadAll")]
        public async Task<object> GetThemeAsync()
        {
            return await this.storage.GetThemeAsync();
        }

        [HttpPut("solution-settings/theme")]
        [Authorize("ReadAll")]
        public async Task<object> SetThemeAsync([FromBody] object theme)
        {
            return await this.storage.SetThemeAsync(theme);
        }

        [HttpGet("solution-settings/logo")]
        [Authorize("ReadAll")]
        public async Task GetLogoAsync()
        {
            var model = await this.storage.GetLogoAsync();
            this.SetImageResponse(model);
        }

        [HttpPut("solution-settings/logo")]
        [Authorize("ReadAll")]
        public async Task SetLogoAsync()
        {
            MemoryStream memoryStream = new MemoryStream();
            this.Request.Body.CopyTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            var model = new Logo
            {
                IsDefault = false
            };

            if (bytes.Length > 0)
            {
                model.SetImageFromBytes(bytes);
                model.Type = this.Request.ContentType;
            }

            if (this.Request.Headers[Logo.NAME_HEADER] != StringValues.Empty)
            {
                model.Name = this.Request.Headers[Logo.NAME_HEADER];
            }

            var response = await this.storage.SetLogoAsync(model);
            this.SetImageResponse(response);
        }

        [HttpGet("solution-settings/actions")]
        public async Task<ActionSettingsListApiModel> GetActionsSettingsAsync()
        {
            var actions = await this.actions.GetListAsync();
            return new ActionSettingsListApiModel(actions);
        }

        private void SetImageResponse(Logo model)
        {
            if (model.Name != null)
            {
                this.Response.Headers.Add(Logo.NAME_HEADER, model.Name);
            }
            this.Response.Headers.Add(Logo.IS_DEFAULT_HEADER, model.IsDefault.ToString());
            this.Response.Headers.Add(SolutionSettingsController.ACCESS_CONTROL_EXPOSE_HEADERS,
                Logo.NAME_HEADER + "," + Logo.IS_DEFAULT_HEADER);
            if (model.Image != null)
            {
                var bytes = model.ConvertImageToBytes();
                this.Response.ContentType = model.Type;
                this.Response.Body.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
