using System;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.External.TimeSeries
{
    public interface ITimeSeriesClient
    {
        Task<StatusResultServiceModel> PingAsync();

        Task<MessageList> QueryEventsAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] deviceIds);
    }
}