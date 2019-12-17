using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.AsaManager.Services.Runtime
{
    public interface IServicesConfig : IIotHubManagerClientConfig, IBlobStorageClientConfig, IStorageAdapterClientConfig
    {
    }

    public class ServicesConfig : IServicesConfig
    {
        public string IotHubManagerApiUrl { get; set; }
        public string StorageAccountConnectionString { get; set; }
        public string StorageAdapterApiUrl { get; set; }
        public int StorageAdapterApiTimeout { get; set; }
    }
}
