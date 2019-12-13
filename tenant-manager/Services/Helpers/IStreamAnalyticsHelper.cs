
using System.Threading.Tasks;
using Microsoft.Azure.Management.StreamAnalytics.Models;
using Mmm.Platform.IoT.Common.Services;

namespace Mmm.Platform.IoT.TenantManager.Services.Helpers
{
    public interface IStreamAnalyticsHelper : IStatusOperation
    {
        Task StartAsync(string saJobName);
        Task StopAsync(string saJobName);
        Task<StreamingJob> GetJobAsync(string saJobName);
        bool JobIsActive(StreamingJob job);
    }
}