using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Common.Services.External.StorageAdapter
{
    public interface IStorageAdapterClient : IStatusOperation
    {
        Task<ValueApiModel> GetAsync(string collectionId, string key);

        Task<ValueListApiModel> GetAllAsync(string collectionId);

        Task<ValueApiModel> CreateAsync(string collectionId, string value);

        Task DeleteAsync(string collectionId, string key);

        Task<ValueApiModel> UpdateAsync(string collectionId, string key, string value, string etag);
    }
}
