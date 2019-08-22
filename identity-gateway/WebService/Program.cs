using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace IdentityGateway.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseUrls("http://*:" + "80")
                .UseKestrel(options => { options.AddServerHeader = false; })
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
