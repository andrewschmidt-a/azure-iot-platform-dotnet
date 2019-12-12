using System.Threading.Tasks;

namespace Mmm.Platform.IoT.TenantManager.Services.Helpers
{
    public interface ITokenHelper
    {
        Task<string> GetTokenAsync();
    }
}