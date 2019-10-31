using System;
using System.Threading.Tasks;
using IdentityGateway.Services.Models;

namespace IdentityGateway.Services.Helpers
{
    public interface IKeyVaultHelpers : IDisposable
    {
        string GetKeyVaultSecretIdentifier(string secret);
        Task<string> GetSecretAsync(string secret);
        Task<StatusResultServiceModel> PingAsync();
    }
}