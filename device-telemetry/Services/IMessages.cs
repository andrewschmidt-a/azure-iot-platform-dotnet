using System;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services
{
    public interface IMessages
    {
        Task<MessageList> ListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);
    }
}
