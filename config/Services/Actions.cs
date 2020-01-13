using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Config.Services.External;
using Mmm.Platform.IoT.Config.Services.Models.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services
{
    public class Actions : IActions
    {
        private readonly IAzureResourceManagerClient resourceManagerClient;
        private readonly ILogger _logger;
        private readonly IActionSettings _emailActionSettings;

        public Actions(
            IAzureResourceManagerClient resourceManagerClient,
            ILogger<Actions> logger,
            IActionSettings emailActionSettings)
        {
            this.resourceManagerClient = resourceManagerClient;
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
