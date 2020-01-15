using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services
{
    public interface ISeed
    {
        Task TrySeedAsync();
    }
}