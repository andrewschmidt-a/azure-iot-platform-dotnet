using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services
{

    public interface IStatusService
    {
        Task<StatusServiceModel> GetStatusAsync();
        IActionResult Ping();
    }
}
