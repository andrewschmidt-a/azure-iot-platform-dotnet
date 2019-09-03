using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityGateway.Services.Runtime
{
    public static class Uptime
    {
        /// <summary>When the service started</summary>
        public static DateTimeOffset Start { get; } = DateTimeOffset.UtcNow;

        /// <summary>How long the service has been running</summary>
        public static TimeSpan Duration => DateTimeOffset.UtcNow.Subtract(Start);

        /// <summary>A randomly generated ID used to identify the process in the logs</summary>
        public static string ProcessId { get; } = "WebService." + Guid.NewGuid();
    }
}
