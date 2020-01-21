using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Helpers
{
    public interface IRsaHelpers
    {
        RSA DecodeRsa(string privateRsaKey);

        JsonWebKeySet GetJsonWebKey(string key);
    }
}