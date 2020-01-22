using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Common.Services.External.AsaManager
{
    public interface IAsaManagerClient : IExternalServiceClient
    {
        Task<BeginConversionApiModel> BeginConversionAsync(string entity);
    }
}
