using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services.Helpers
{
    public interface IHttpClientWrapper
    {
        Task<T> GetAsync<T>(string uri, string description, bool acceptNotFound = false);

        Task PostAsync(string uri, string description, object content = null);

        Task PutAsync(string uri, string description, object content = null);

        void SetHeaders(Dictionary<string, string> headers);
    }
}