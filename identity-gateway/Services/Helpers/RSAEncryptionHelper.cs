
using Org.BouncyCastle.Crypto; // Because why wouldnt you use a bouncy castle??? #NeverTooOld
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography;

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

    }
}