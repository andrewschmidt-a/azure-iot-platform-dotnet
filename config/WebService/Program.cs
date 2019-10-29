// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.Runtime;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Mmm.Platform.IoT.Common.WebService.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService
{
    public class Program
    {
        // Application entry point
        public static void Main(string[] args)
        {
            var config = new Config(new ConfigData(new Logger(Uptime.ProcessId, LogLevel.Info)));

            /*
            Kestrel is a cross-platform HTTP server based on libuv,
            a cross-platform asynchronous I/O library.
            https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers
            */
            var host = new WebHostBuilder()
                .UseUrls("http://*:" + config.Port)
                .UseKestrel(options => { options.AddServerHeader = false; })
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
