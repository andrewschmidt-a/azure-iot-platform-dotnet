using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers
{
    public class AppConfigurationHelper
    {
        public static IConfigurationRoot GetAppConfig(string appconfigconnection)
        {
            var builder = new ConfigurationBuilder();
            builder.AddAzureAppConfiguration(appconfigconnection);
            return builder.Build();
        }
    }
}
