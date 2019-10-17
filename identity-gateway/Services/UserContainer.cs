using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityGateway.AuthUtils;
using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;

namespace IdentityGateway.Services
{
    public interface IUserContainer<TModel, UInput>
    {
        Task<TModel> GetAsync(UInput input);
        Task<TModel> CreateAsync(UInput input);
        Task<TModel> UpdateAsync(UInput input);
        Task<TModel> DeleteAsync(UInput input);
    }

    public abstract class UserContainer
    {
        // injections
        protected TableHelper _tableHelper;

        // abstracts
        abstract public string tableName { get; }

        public UserContainer(TableHelper tableHelper)
        {
            this._tableHelper = tableHelper;
        }

        public async Task<StatusResultServiceModel> PingAsync()
        {
            await this._tableHelper.GetTableAsync(this.tableName);
            return new StatusResultServiceModel(true, "Alive and Well!");
        }
    }
}