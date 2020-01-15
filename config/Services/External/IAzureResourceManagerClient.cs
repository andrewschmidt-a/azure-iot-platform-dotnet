using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public interface IAzureResourceManagerClient
    {
        Task<bool> IsOffice365EnabledAsync();
    }
}
