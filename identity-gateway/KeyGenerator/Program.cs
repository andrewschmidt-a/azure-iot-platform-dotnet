using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Mmm.Platform.IoT.IdentityGateway.KeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {

            string key = string.Empty;
            using (var memStream = new MemoryStream())
            {
                // Generate a public/private key pair.
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
                // Save the public key information to an RSAParameters structure.
                RSAParameters rsaKeyInfo = rsa.ExportParameters(true);

                AsymmetricCipherKeyPair KeyPair = DotNetUtilities.GetRsaKeyPair(rsaKeyInfo);
                TextWriter textWriter = new StringWriter();
                PemWriter pemWriter = new PemWriter(textWriter);
                pemWriter.WriteObject(KeyPair.Private);
                pemWriter.WriteObject(KeyPair.Public);
                pemWriter.Writer.Flush();
                key = textWriter.ToString();
            }

            Console.WriteLine(key);
            Console.WriteLine(key.Replace("\r\n", "\\n"));
            Console.ReadLine();
        }
    }
}
