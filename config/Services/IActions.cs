using Mmm.Platform.IoT.Config.Services.Models.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services
{
    public interface IActions
    {
        Task<List<IActionSettings>> GetListAsync();
    }
}
