using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Http;

namespace Mmm.Platform.IoT.Common.Services.Helpers
{
    public interface IExternalRequestHelper
    {
        Task<T> ProcessRequestAsync<T>(HttpMethod method, string url, string tenantId = null);
        Task<T> ProcessRequestAsync<T>(HttpMethod method, string url, T content, string tenantId = null);
        Task<IHttpResponse> ProcessRequestAsync(HttpMethod method, string url, string tenantId = null);
        Task<T> SendRequestAsync<T>(HttpMethod method, IHttpRequest request);
        Task<IHttpResponse> SendRequestAsync(HttpMethod method, IHttpRequest request);
    }
}