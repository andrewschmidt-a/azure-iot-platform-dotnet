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
using JsonWebKey = IdentityModel.Jwk.JsonWebKey;
using JsonWebKeySet = IdentityModel.Jwk.JsonWebKeySet;

namespace IdentityGateway.WebService.v1.Controllers
{
    [Route("")]
    public class ConfiguratonController : ControllerBase
    {
        private IConfiguration _config;
        public ConfiguratonController(IConfiguration config)
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
            var publicKey = "";
            /* Get Secrets From KeyVault */
            using (KeyVaultHelper kvh = new KeyVaultHelper(this._config))
            {
                publicKey = (await kvh.getSecretAsync("identityGatewayPublicKey")).Replace("\\n", "\n");
            }
                JsonWebKeySet jsonWebKeySet = new JsonWebKeySet();
            using (var textReader = new StringReader(publicKey))
            {
                var pubkeyReader = new PemReader(textReader);
                RsaKeyParameters KeyParameters = (RsaKeyParameters)pubkeyReader.ReadObject();
                var e = Base64UrlEncoder.Encode(KeyParameters.Exponent.ToByteArrayUnsigned());
                var n = Base64UrlEncoder.Encode(KeyParameters.Modulus.ToByteArrayUnsigned());
                var dict = new Dictionary<string, string>() {
                    {"e", e},
                    {"kty", "RSA"},
                    {"n", n}
                };
                var hash = SHA256.Create();
                Byte[] hashBytes = hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dict)));
                JsonWebKey jsonWebKey = new JsonWebKey()
                {
                    Kid = Base64UrlEncoder.Encode(hashBytes),
                    Kty = "RSA",
                    E = e,
                    N = n
                };
                jsonWebKeySet.Keys.Add(jsonWebKey);
            }
            return new ContentResult() {Content = JsonConvert.SerializeObject(jsonWebKeySet), ContentType = "application/json"};
        }
    }
}
