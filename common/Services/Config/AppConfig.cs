using Microsoft.Extensions.Configuration;

namespace Mmm.Platform.IoT.Common.Services.Config
{
    public partial class AppConfig
    {
        public AppConfig() { }

        public AppConfig(IConfiguration configuration)
        {
            configuration.Bind(this);
            Configuration = configuration;
        }

        public AppConfig(IConfigurationBuilder configurationBuilder)
            : this(configurationBuilder.Build())
        {
        }

        public IConfiguration Configuration { get; private set; }
        public string AppConfigurationConnectionString { get; set; }
        public string ASPNETCORE_URLS { get; set; }
    }
}