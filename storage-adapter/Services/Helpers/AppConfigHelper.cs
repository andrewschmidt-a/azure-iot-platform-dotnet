using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers
{
    public interface IAppConfigurationHelper
    {
        IConfigurationRoot GetAppConfig();
    }

    public class AppConfigurationHelper : IAppConfigurationHelper
    {
        public string appConfigConnectionString;

        public AppConfigurationHelper(string appConfigConnectionString)
        {
            this.appConfigConnectionString = appConfigConnectionString;
        }

        public IConfigurationRoot GetAppConfig()
        {
            var builder = new ConfigurationBuilder();
            builder.AddAzureAppConfiguration(this.appConfigConnectionString);
            return builder.Build();
        }
    }
}