using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Common.Services.External
{
    public interface IUserManagementClient : IStatusOperation
    {
        Task<IEnumerable<string>> GetAllowedActionsAsync(string userObjectId, IEnumerable<string> roles);

        Task<string> GetTokenAsync();
    }
}