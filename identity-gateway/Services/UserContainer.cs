using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public abstract class UserContainer : IStatusOperation
    {
        public UserContainer()
        {
        }

        public UserContainer(ITableStorageClient tableStorageClient)
        {
            this.TableStorageClient = tableStorageClient;
        }

        public abstract string TableName { get; }

        protected ITableStorageClient TableStorageClient { get; set; }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            await this.TableStorageClient.GetTableAsync(this.TableName);
            return new StatusResultServiceModel(true, "Alive and Well!");
        }
    }
}