// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.Config.Services;

namespace Mmm.Platform.IoT.Config.WebService.v1.Controllers
{
    [Route("v1/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class SeedController : Controller
    {
        private readonly ISeed seed;

        public SeedController(ISeed seed)
        {
            this.seed = seed;
        }

        [HttpPost]
        [Authorize("ReadAll")]
        public async Task PostAsync()
        {
            await this.seed.TrySeedAsync();
        }
    }
}
