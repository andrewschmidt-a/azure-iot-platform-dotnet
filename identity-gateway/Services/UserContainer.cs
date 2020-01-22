using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public abstract class UserContainer
    {
        // injections
        protected ITableStorageClient _tableStorageClient;

        // abstracts
        public abstract string TableName { get; }

        public UserContainer()
        {
        }

        public UserContainer(ITableStorageClient tableStorageClient)
        {
            this._tableStorageClient = tableStorageClient;
        }
    }
}