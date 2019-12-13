using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Mmm.Platform.IoT.IdentityGateway.WebService.Runtime;

namespace Mmm.Platform.IoT.IdentityGateway.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new Config(new ConfigData());
            var host = new WebHostBuilder()
                .UseUrls("http://*:" + config.Port)
                .UseKestrel(options => { options.AddServerHeader = false; })
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureLogging(builder => builder.AddConsole())
                .Build();

            host.Run();
        }
    }
}
