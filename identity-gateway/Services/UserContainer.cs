using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public abstract class UserContainer : IStatusOperation
    {
        protected ITableStorageClient tableStorageClient;

        public UserContainer()
        {
        }

        public UserContainer(ITableStorageClient tableStorageClient)
        {
            this.tableStorageClient = tableStorageClient;
        }

        public abstract string TableName { get; }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            await this.tableStorageClient.GetTableAsync(this.TableName);
            return new StatusResultServiceModel(true, "Alive and Well!");
        }
    }
}