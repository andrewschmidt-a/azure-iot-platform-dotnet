using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.DeviceTelemetry.Services.Models;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services
{
    public interface IAlarms
    {
        Task<Alarm> GetAsync(string id);

        Task<List<Alarm>> ListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);

        Task<List<Alarm>> ListByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);

        Task<int> GetCountByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string[] devices);

        Task<Alarm> UpdateAsync(string id, string status);

        Task Delete(List<string> ids);

        Task DeleteAsync(string id);
    }
}
