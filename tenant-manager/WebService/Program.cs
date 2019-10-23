using MMM.Azure.IoTSolutions.TenantManager.Services.Diagnostics;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;
using MMM.Azure.IoTSolutions.TenantManager.WebService.Runtime;
using Microsoft.AspNetCore.Hosting;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService
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
