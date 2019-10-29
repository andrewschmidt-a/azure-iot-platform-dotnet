namespace Mmm.Platform.IoT.Common.Services.External.StorageAdapter
{
    public interface IStorageAdapterClientConfig
    {
        string StorageAdapterApiUrl { get; set; }
        int StorageAdapterApiTimeout { get; set; }
    }
}