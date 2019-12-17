using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Runtime;

namespace Mmm.Platform.IoT.TenantManager.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new Runtime.Config(new ConfigData());
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
