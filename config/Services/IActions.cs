using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Config.Services.Models.Actions;

namespace Mmm.Platform.IoT.Config.Services
{
    public interface IActions
    {
        Task<List<IActionSettings>> GetListAsync();
    }
}