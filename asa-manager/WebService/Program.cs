using Microsoft.AspNetCore.Hosting;
using Mmm.Platform.IoT.Common.Services;

namespace Mmm.Platform.IoT.AsaManager.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);
            builder.UseStartup<Startup>();
            var host = builder.Build();
            host.Run();
        }
    }
}