using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Http;
using Newtonsoft.Json;
using HttpRequest = Mmm.Platform.IoT.Common.Services.Http.HttpRequest;

namespace Mmm.Platform.IoT.Common.Services.Helpers
{
    public class ExternalRequestHelper : IExternalRequestHelper
    {
        private const string TENANT_HEADER = "ApplicationTenantID";
        private const string AZDS_ROUTE_KEY = "azds-route-as";

        private readonly IHttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExternalRequestHelper(IHttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            this._httpClient = httpClient;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Process an External Dependency Request using the given parameters to create a generic HttpRequest and deserialize the response to type T
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="tenantId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ProcessRequestAsync<T>(HttpMethod method, string url, string tenantId = null)
        {
            IHttpRequest request = this.CreateRequest(url, tenantId);
            return await this.SendRequestAsync<T>(method, request);
        }

        /// <summary>
        /// Process an External Dependency Request using the given parameters to create a generic HttpRequest and deserialize the body and response to type T
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="tenantId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ProcessRequestAsync<T>(HttpMethod method, string url, T content, string tenantId = null)
        {
            IHttpRequest request = this.CreateRequest(url, content, tenantId);
            return await this.SendRequestAsync<T>(method, request);
        }

        /// <summary>
        /// Process an External Dependency Request using the given parameters to create a generic HttpRequest and return the response.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<IHttpResponse> ProcessRequestAsync(HttpMethod method, string url, string tenantId = null)
        {
            IHttpRequest request = this.CreateRequest(url, tenantId);
            return await this.SendRequestAsync(method, request);
        }

        /// <summary>
        /// Send an HttpRequest using the given HTTP method, deserialize the response to type T
        /// </summary>
        /// <param name="method"></param>
        /// <param name="request"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> SendRequestAsync<T>(HttpMethod method, IHttpRequest request)
        {
            IHttpResponse response = await this.SendRequestAsync(method, request);
            string responseContent = response?.Content?.ToString();
            try
            {
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception e)
            {
                throw new JsonReaderException("Unable to deserialize response content to the proper API model.", e);
            }
        }

        /// <summary>
        /// Send an HttpRequest using the given HTTP method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IHttpResponse> SendRequestAsync(HttpMethod method, IHttpRequest request)
        {
            IHttpResponse response = null;
            try
            {
                response = await this._httpClient.SendAsync(request, method);
            }
            catch (Exception e)
            {
                throw new HttpRequestException("An error occurred while sending the request.", e);
            }

            this.ThrowIfError(response, request);
            return response;
        }

        /// <summary>
        /// Create an HttpRequest with the necessary parameters for an External Dependency API request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        private IHttpRequest CreateRequest(string url, string tenantId = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString(url);

            if (string.IsNullOrEmpty(tenantId))
            {
                try
                {
                    tenantId = this._httpContextAccessor.HttpContext.Request.GetTenant();
                }
                catch (Exception e)
                {
                    throw new ArgumentException("The tenantId for the External Request was not provided and could not be retrieved from the HttpContextAccessor Request.", e);
                }
            }

            request.AddHeader(TENANT_HEADER, tenantId);

            if (url.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (this._httpContextAccessor.HttpContext != null && this._httpContextAccessor.HttpContext.Request.Headers.ContainsKey(AZDS_ROUTE_KEY))
            {
                try
                {
                    var azdsRouteAs = this._httpContextAccessor.HttpContext.Request.Headers.First(p => string.Equals(p.Key, AZDS_ROUTE_KEY, StringComparison.OrdinalIgnoreCase));
                    request.Headers.Add(AZDS_ROUTE_KEY, azdsRouteAs.Value.First());  // azdsRouteAs.Value returns an iterable of strings, take the first
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to attach the {AZDS_ROUTE_KEY} header to the IdentityGatewayClient Request.", e);
                }
            }

            return request;
        }

        /// <summary>
        /// Create an HttpRequest with the necessary parameters for an External Dependency API request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="tenantId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IHttpRequest CreateRequest<T>(string url, T content, string tenantId)
        {
            IHttpRequest request = this.CreateRequest(url, tenantId);
            request.SetContent(content);
            return request;
        }

        private void ThrowIfError(IHttpResponse response, IHttpRequest request)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException(response.Content);

                case HttpStatusCode.Conflict:
                    throw new ConflictingResourceException(response.Content);

                default:
                    throw new HttpRequestException(response.Content);
            }
        }
    }
}