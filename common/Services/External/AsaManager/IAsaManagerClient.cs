using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Common.Services.External.AsaManager
{
    public interface IAsaManagerClient : IStatusOperation
    {
        Task<BeginConversionApiModel> BeginConversionAsync(string entity);
    }
}