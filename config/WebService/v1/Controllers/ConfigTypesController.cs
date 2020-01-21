// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.Config.Services;
using Mmm.Platform.IoT.Config.WebService.v1.Models;

namespace Mmm.Platform.IoT.Config.WebService.v1.Controllers
{
    [Route("v1/configtypes")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class ConfigTypesController
    {
        private readonly IStorage storage;

        public ConfigTypesController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<ConfigTypeListApiModel> GetAllConfigTypesAsync()
        {
            return new ConfigTypeListApiModel(await this.storage.GetConfigTypesListAsync());
        }
    }
}
