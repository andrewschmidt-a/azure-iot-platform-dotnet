using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services
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