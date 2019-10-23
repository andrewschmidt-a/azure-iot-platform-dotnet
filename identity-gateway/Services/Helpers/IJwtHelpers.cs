using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IdentityGateway.Services.Helpers
{
    public interface IJwtHelpers
    {
        Task<JwtSecurityToken> GetIdentityToken(List<Claim> claims, string tenant, string audience, DateTime? expiration);
        JwtSecurityToken MintToken(List<Claim> claims, string audience, DateTime expirationDateTime);
        bool TryValidateToken(string audience, string encodedToken, HttpContext context, out JwtSecurityToken jwt);
    }
}