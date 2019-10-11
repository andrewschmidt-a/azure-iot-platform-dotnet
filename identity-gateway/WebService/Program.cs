using IdentityGateway.Services.Diagnostics;
using IdentityGateway.Services.Runtime;
using IdentityGateway.WebService.Runtime;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace IdentityGateway.WebService
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
