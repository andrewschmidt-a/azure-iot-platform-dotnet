using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers
{
    public interface IAppConfigurationHelper
    {
        IConfigurationRoot GetAppConfig(string appconfigconnection);
    }
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