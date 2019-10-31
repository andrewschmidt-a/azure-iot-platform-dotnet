using System.Threading.Tasks;

namespace IdentityGateway.Services
{
    public interface IUserContainer<TModel, UInput>
    {
        Task<TModel> GetAsync(UInput input);
        Task<TModel> CreateAsync(UInput input);
        Task<TModel> UpdateAsync(UInput input);
        Task<TModel> DeleteAsync(UInput input);
    }
}