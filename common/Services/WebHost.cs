using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using System.IO;
using System.Reflection;

namespace Mmm.Platform.IoT.Common.Services
{
    public class WebHost
    {
        public static IWebHostBuilder CreateDefaultBuilder(string[] args)
        {
            var builder = new WebHostBuilder();
            builder.UseEnvironment(EnvironmentName.Development);
            UseContentRoot(builder);
            ConfigureHostConfiguration(args, builder);
            ConfigureAppConfiguration(args, builder);
            ConfigureLogging(builder);
            builder.UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = context.HostingEnvironment.EnvironmentName == EnvironmentName.Development;
            });
            builder.UseKestrel(options => { options.AddServerHeader = false; });
            builder.UseIISIntegration();
            return builder;
        }

        private static void ConfigureLogging(WebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            });
        }

        private static void ConfigureAppConfiguration(string[] args, WebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
                var configurationBuilder = new ConfigurationBuilder();

                if (env.EnvironmentName == EnvironmentName.Development || env.EnvironmentName == EnvironmentName.Qa)
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly != null)
                    {
                        configurationBuilder.AddUserSecrets(appAssembly, optional: true);
                    }
                }

                configurationBuilder.AddEnvironmentVariables();
                if (args != null)
                {
                    configurationBuilder.AddCommandLine(args);
                }

                var initialAppConfig = new AppConfig(configurationBuilder);
                configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddAzureAppConfiguration(initialAppConfig.PCS_APPLICATION_CONFIGURATION);
                var azureAppConfigConfig = new AppConfig(configurationBuilder);
                config.AddConfiguration(azureAppConfigConfig.Configuration);
                config.AddAzureKeyVault(
                    $"https://{azureAppConfigConfig.KeyVault.Name}.vault.azure.net/",
                    azureAppConfigConfig.Global.AzureActiveDirectory.AadAppId,
                    azureAppConfigConfig.Global.AzureActiveDirectory.AadAppSecret);
                config.AddConfiguration(initialAppConfig.Configuration);
            });
        }

        private static void ConfigureHostConfiguration(string[] args, WebHostBuilder builder)
        {
            var hostConfigurationBuilder = new ConfigurationBuilder();
            hostConfigurationBuilder.AddEnvironmentVariables();
            if (args != null)
            {
                hostConfigurationBuilder.AddCommandLine(args);
            }

            builder.UseConfiguration(hostConfigurationBuilder.Build());
        }

        private static void UseContentRoot(WebHostBuilder builder)
        {
            if (string.IsNullOrEmpty(builder.GetSetting(WebHostDefaults.ContentRootKey)))
            {
                builder.UseContentRoot(Directory.GetCurrentDirectory());
            }
        }
    }
}
