using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using Newtonsoft.Json.Serialization;
using JsonWebKey = IdentityModel.Jwk.JsonWebKey;
using JsonWebKeySet = IdentityModel.Jwk.JsonWebKeySet;
using IdentityGateway.Services.Helpers;
using RSA = IdentityGateway.Services.Helpers.RSA;
using IdentityGateway.Services.Runtime;

namespace IdentityGateway.WebService.v1.Controllers
{
    [Route("")]
    public class ConfiguratonController : ControllerBase
    {
        private IServicesConfig _config;
        public ConfiguratonController(IServicesConfig config)
        {
            this._config = config;
        }
        // GET api/values
        [HttpGet(".well-known/openid-configuration")]
        public ContentResult Get()
        {
            return new ContentResult() {Content = JsonConvert.SerializeObject(new Configuration(HttpContext)), ContentType = "application/json"}; 
        }

        // GET api/values
        [HttpGet(".well-known/openid-configuration/jwks")]
        public async Task<ContentResult> GetAsync()
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new LowercaseContractResolver();
            
            return new ContentResult() {Content = JsonConvert.SerializeObject(RSA.GetJsonWebKey(this._config.PublicKey), serializerSettings), ContentType = "application/json"};
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