
using Org.BouncyCastle.Crypto; // Because why wouldnt you use a bouncy castle??? #NeverTooOld
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace IdentityGateway.Services.Helpers
{
    public class RSA
    {
        public static System.Security.Cryptography.RSA DecodeRSA(string privateRsaKey)
        {
            RSAParameters rsaParams;
            using (var tr = new StringReader(privateRsaKey.Replace("\\n", "\n")))
            {
                var pemReader = new PemReader(tr);
                var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                if (keyPair == null)
                {
                    throw new Exception("Could not read RSA private key");
                }
                var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
            }

            System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportParameters(rsaParams);
            return rsa;
        }

        public static async Task<JsonWebKeySet> GetJsonWebKey(IConfiguration config)
        {
            var publicKey = "";
            /* Get Secrets From KeyVault */
            using (KeyVaultHelper kvh = new KeyVaultHelper(config))
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

            return jsonWebKeySet;
        }

    }
}