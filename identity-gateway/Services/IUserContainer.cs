using System.Threading.Tasks;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public interface IUserContainer<TModel, TUiInput>
    {
        Task<TModel> GetAsync(TUiInput input);

        Task<TModel> CreateAsync(TUiInput input);

        Task<TModel> UpdateAsync(TUiInput input);

        Task<TModel> DeleteAsync(TUiInput input);
    }
}