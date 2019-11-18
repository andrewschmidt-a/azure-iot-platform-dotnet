using System.Threading.Tasks;
using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using Mmm.Platform.IoT.Common.Services.Models;

namespace IdentityGateway.Services
{
    public abstract class UserContainer
    {
        // injections
        protected ITableHelper _tableHelper;

        // abstracts
        public abstract string TableName { get; }

        public UserContainer()
        {
        }

        public UserContainer(ITableHelper tableHelper)
        {
            this._tableHelper = tableHelper;
        }

        public async Task<StatusResultServiceModel> PingAsync()
        {
            await this._tableHelper.GetTableAsync(this.TableName);
            return new StatusResultServiceModel(true, "Alive and Well!");
        }
    }
}