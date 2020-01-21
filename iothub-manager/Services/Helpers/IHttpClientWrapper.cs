using System.Threading.Tasks;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Helpers
{
    public interface IHttpClientWrapper
    {
        Task PostAsync(string uri, string description, object content = null);
    }
}