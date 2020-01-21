using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Config.Services.External;
using Mmm.Platform.IoT.Config.Services.Models.Actions;

namespace Mmm.Platform.IoT.Config.Services
{
    public class Actions : IActions
    {
        private readonly IAzureResourceManagerClient resourceManagerClient;
        private readonly ILogger logger;
        private readonly IActionSettings emailActionSettings;

        public Actions(
            IAzureResourceManagerClient resourceManagerClient,
            ILogger<Actions> logger,
            IActionSettings emailActionSettings)
        {
            this.resourceManagerClient = resourceManagerClient;
            this.logger = logger;
            this.emailActionSettings = emailActionSettings;
        }

        public async Task<List<IActionSettings>> GetListAsync()
        {
            var result = new List<IActionSettings>();
            await emailActionSettings.InitializeAsync();
            result.Add(emailActionSettings);

            return result;
        }
    }
}
