using System.Threading.Tasks;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services
{
    public interface IAlertingContainer
    {
        Task<StreamAnalyticsJobModel> AddAlertingAsync(string tenantId);
        Task<StreamAnalyticsJobModel> RemoveAlertingAsync(string tenantId);
        Task<StreamAnalyticsJobModel> GetAlertingAsync(string tenantId);
        Task<StreamAnalyticsJobModel> StartAlertingAsync(string tenantId);
        Task<StreamAnalyticsJobModel> StopAlertingAsync(string tenantId);
        bool SaJobExists(StreamAnalyticsJobModel model);
    }
}