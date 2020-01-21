using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services
{
    public interface IRules
    {
        Task CreateFromTemplateAsync(string template);

        Task DeleteAsync(string id);

        Task<Rule> GetAsync(string id);

        Task<List<Rule>> GetListAsync(
            string order,
            int skip,
            int limit,
            string groupId,
            bool includeDeleted);

        Task<List<AlarmCountByRule>> GetAlarmCountForListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);

        Task<Rule> CreateAsync(Rule rule);

        Task<Rule> UpsertIfNotDeletedAsync(Rule rule);
    }
}