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
        Task<List<TModel>> GetAllAsync(UInput input);
        Task<TModel> GetAsync(UInput input);
        Task<TModel> CreateAsync(UInput input);
        Task<TModel> UpdateAsync(UInput input);
        Task<TModel> DeleteAsync(UInput input);
    }

    public abstract class UserContainer
    {
        // injections
        private IHttpContextAccessor _httpContextAccessor;
        protected TableHelper _tableHelper;

        // abstracts
        abstract public string tableName { get; }

        public UserContainer(IHttpContextAccessor httpContextAccessor, TableHelper tableHelper)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._tableHelper = tableHelper;
        }

        protected string tenant
        {
            // get the tenant guid from the http context - this utilizes AuthUtil's request extension
            get
            {
                return this._httpContextAccessor.HttpContext.Request.GetTenant();
            }
        }

        public async Task<StatusResultServiceModel> PingAsync()
        {
            await this._tableHelper.GetTableAsync(this.tableName);
            return new StatusResultServiceModel(true, "Alive and Well!");
        }

        public async Task<bool> RecordExists(TableEntity model)
        {
            return await this.GetAsync(model) != null;
        }

        abstract public async Task<TableEntity> GetAsync(IUserInput input)
        {
        }
    }
}