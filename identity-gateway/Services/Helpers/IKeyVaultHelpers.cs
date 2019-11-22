using System;
using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Helpers
{
    public interface IKeyVaultHelpers : IDisposable
    {
        string GetKeyVaultSecretIdentifier(string secret);
        Task<string> GetSecretAsync(string secret);
        Task<StatusResultServiceModel> PingAsync();
    }
}