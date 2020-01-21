using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Mmm.Platform.IoT.IdentityGateway.KeyGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string key = string.Empty;
            using (var memStream = new MemoryStream())
            {
                // Generate a public/private key pair.
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);

                // Save the public key information to an RSAParameters structure.
                RSAParameters rsaKeyInfo = rsa.ExportParameters(true);

                AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetRsaKeyPair(rsaKeyInfo);
                TextWriter textWriter = new StringWriter();
                PemWriter pemWriter = new PemWriter(textWriter);
                pemWriter.WriteObject(keyPair.Private);
                pemWriter.WriteObject(keyPair.Public);
                pemWriter.Writer.Flush();
                key = textWriter.ToString();
            }

            Console.WriteLine(key);
            Console.WriteLine(key.Replace("\r\n", "\\n"));
            Console.ReadLine();
        }
    }
}