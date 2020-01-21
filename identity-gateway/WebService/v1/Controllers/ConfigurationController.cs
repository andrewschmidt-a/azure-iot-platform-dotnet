using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.v1.Controllers
{
    [Route("")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class ConfigurationController : Controller
    {
        private AppConfig config;
        private readonly IOpenIdProviderConfiguration _openIdProviderConfiguration;
        private readonly IRsaHelpers _rsaHelpers;
        public const string ContentType = "application/json";

        public ConfigurationController(AppConfig config, IOpenIdProviderConfiguration openIdProviderConfiguration, IRsaHelpers rsaHelpers)
        {
            this.config = config;
            _openIdProviderConfiguration = openIdProviderConfiguration;
            _rsaHelpers = rsaHelpers;
        }

        /// <summary>
        /// From https://openid.net/specs/openid-connect-discovery-1_0.html#ProviderConfig:
        ///     OpenID Providers supporting Discovery MUST make a JSON
        ///     document available at the path formed by concatenating
        ///     the string /.well-known/openid-configuration to the Issuer.
        ///     The syntax and semantics of .well-known are defined in RFC
        ///     5785 [RFC5785] and apply to the Issuer value when it contains
        ///     no path component. openid-configuration MUST point to a JSON
        ///     document compliant with this specification and MUST be
        ///     returned using the application/json content type.
        /// </summary>
        /// <seealso cref="http://tools.ietf.org/html/rfc5785"/>
        /// <returns>
        /// The response is a set of Claims about the OpenID Provider's
        /// configuration, including all necessary endpoints and public key
        /// location information. A successful response MUST use the 200 OK
        /// HTTP status code and return a JSON object using the
        /// application/json content type that contains a set of Claims as its
        /// members that are a subset of the Metadata values defined in
        /// Section 3. Other Claims MAY also be returned.
        /// Claims that return multiple values are represented as JSON arrays.
        /// Claims with zero elements MUST be omitted from the response.
        /// An error response uses the applicable HTTP status code value.
        /// </returns>
        [HttpGet(".well-known/openid-configuration")]
        public IActionResult GetOpenIdProviderConfiguration()
        {
            return new OkObjectResult(_openIdProviderConfiguration) { ContentTypes = new MediaTypeCollection { ContentType } };
        }

        // GET api/values
        [HttpGet(".well-known/openid-configuration/jwks")]
        public ContentResult GetJsonWebKeySet()
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new LowercaseContractResolver();
            return new ContentResult() { Content = JsonConvert.SerializeObject(_rsaHelpers.GetJsonWebKey(config.IdentityGatewayService.PublicKey), serializerSettings), ContentType = ContentType, StatusCode = StatusCodes.Status200OK };
        }
    }
}

public class LowercaseContractResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        return propertyName.ToLower();
    }
}