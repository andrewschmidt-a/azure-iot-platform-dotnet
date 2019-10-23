using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace IdentityGateway.Services.Helpers
{
    public interface IRsaHelpers
    {
        RSA DecodeRsa(string privateRsaKey);
        JsonWebKeySet GetJsonWebKey(string key);
    }
}