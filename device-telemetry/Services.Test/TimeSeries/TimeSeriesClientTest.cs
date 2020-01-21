using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Xunit;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Test.TimeSeries
{
    public class TimeSeriesClientTest
    {
        private readonly Mock<ILogger<TimeSeriesClient>> logger;
        private readonly Mock<IHttpClient> httpClient;
        private Mock<AppConfig> config;
        private TimeSeriesClient client;

        public TimeSeriesClientTest()
        {
            logger = new Mock<ILogger<TimeSeriesClient>>();
            this.config = new Mock<AppConfig> { DefaultValue = DefaultValue.Mock };
            this.httpClient = new Mock<IHttpClient>();
            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.config.Object,
                logger.Object);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task QueryThrowsInvalidConfiguration_WhenConfigValuesAreNull()
        {
            // Arrange
            this.SetupClientWithNullConfigValues();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidConfigurationException>(() =>
                 this.client.QueryEventsAsync(null, null, "desc", 0, 1000, new string[0]));
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PingReturnsFalse_WhenConfigValuesAreNull()
        {
            // Arrange
            this.SetupClientWithNullConfigValues();

            // Act
            var result = await this.client.StatusAsync();

            // Assert
            Assert.False(result.IsHealthy);
            Assert.Contains("TimeSeries check failed", result.Message);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task QueryThrows_IfInvalidAuthParams()
        {
            // Arrange
            this.SetupClientWithConfigValues();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidConfigurationException>(() =>
                this.client.QueryEventsAsync(null, null, "desc", 0, 1000, new string[0]));
        }

        private void SetupClientWithNullConfigValues()
        {
            this.config = new Mock<AppConfig> { DefaultValue = DefaultValue.Mock };
            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.config.Object,
                logger.Object);
        }

        private void SetupClientWithConfigValues()
        {
            this.config.Setup(f => f.DeviceTelemetryService.TimeSeries.TsiDataAccessFqdn).Returns("test123");
            this.config.Setup(f => f.DeviceTelemetryService.TimeSeries.Audience).Returns("test123");
            this.config.Setup(f => f.DeviceTelemetryService.TimeSeries.ApiVersion).Returns("2016-12-12-test");
            this.config.Setup(f => f.DeviceTelemetryService.TimeSeries.Timeout).Returns("PT20S");
            this.config.Setup(f => f.Global.AzureActiveDirectory.TenantId).Returns("test123");
            this.config.Setup(f => f.Global.AzureActiveDirectory.AppId).Returns("test123");
            this.config.Setup(f => f.Global.AzureActiveDirectory.AppSecret).Returns("test123");
            this.config.Setup(f => f.DeviceTelemetryService.TimeSeries.Authority).Returns("https://login.testing.net/");

            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.config.Object,
                logger.Object);
        }
    }
}
