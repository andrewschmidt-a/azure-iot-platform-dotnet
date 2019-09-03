using System.Threading.Tasks;
using IdentityGateway.Services;
using IdentityGateway.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IdentityGateway.WebService.v1.Controllers
{
    [Route("api/[controller]")]
    public sealed class StatusController : Controller
    {
        private readonly IConfiguration config;
        private readonly IStatusService statusService;

        public StatusController(IConfiguration config, IStatusService statusService)
        {
            this.config = config;
            this.statusService = statusService;
        }

        public async Task<StatusApiModel> GetAsync()
        {
            var result = new StatusApiModel(await this.statusService.GetStatusAsync());

            result.Properties.Add("Port", this.config["Port"].ToString());
            return result;
        }
    }
}