using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Helpers
{
    public interface ITableHelper
    {
        Task<CloudTable> GetTableAsync(string tableName);
        Task<TableQuerySegment> QueryAsync(string tableName, TableQuery query, TableContinuationToken token);
        Task<TableResult> ExecuteOperationAsync(string tableName, TableOperation operation);
    }
}