using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;

namespace Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage
{
    public interface IBlobStorageClient : IStatusOperation
    {
        Task CreateBlobAsync(string blobContainerName, string contentFileName, string blobFileName);
    }
}