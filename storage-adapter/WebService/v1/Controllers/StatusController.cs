// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.StorageAdapter.WebService.v1.Models;

namespace Mmm.Platform.IoT.StorageAdapter.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IStatusService statusService;

        public StatusController(IStatusService statusService)
        {
            this.statusService = statusService;
        }

        [HttpGet]
        public async Task<StatusApiModel> GetAsync()
        {
            return new StatusApiModel(await this.statusService.GetStatusAsync(false));
        }
    }
}
