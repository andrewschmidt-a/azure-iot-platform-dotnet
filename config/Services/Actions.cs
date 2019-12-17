// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Config.Services.External;
using Mmm.Platform.IoT.Config.Services.Models.Actions;
using Mmm.Platform.IoT.Config.Services.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services
{
    public interface IActions
    {
        Task<List<IActionSettings>> GetListAsync();
    }

    public class Actions : IActions
    {
        private readonly IAzureResourceManagerClient resourceManagerClient;
        private readonly IServicesConfig servicesConfig;
        private readonly ILogger _logger;
        private readonly EmailActionSettings _emailActionSettings;

        public Actions(
            IAzureResourceManagerClient resourceManagerClient,
            IServicesConfig servicesConfig,
            ILogger<Actions> logger,
            EmailActionSettings emailActionSettings)
        {
            this.resourceManagerClient = resourceManagerClient;
            this.servicesConfig = servicesConfig;
            _logger = logger;
            _emailActionSettings = emailActionSettings;
        }

        public async Task<List<IActionSettings>> GetListAsync()
        {
            var result = new List<IActionSettings>();
            await _emailActionSettings.InitializeAsync();
            result.Add(_emailActionSettings);

            return result;
        }
    }
}
