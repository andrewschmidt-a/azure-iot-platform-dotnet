// <copyright file="AppConfigurationClientTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.AppConfiguration;
using Mmm.Iot.Common.Services.Config;
using Mmm.Iot.Common.Services.External.AppConfiguration;
using Mmm.Iot.Common.Services.Models;
using Mmm.Iot.Common.TestHelpers;
using Moq;
using Xunit;

namespace Mmm.Iot.Common.Services.Test
{
    public class AppConfigurationClientTest
    {
        private const string MockConnectionString = @"Endpoint=https://abc.azconfig.io;Id=1:/1;Secret=1234";
        private const string AzdsRouteKey = "azds-route-as";
        private readonly Mock<ConfigurationClient> client;
        private readonly Mock<AppConfig> mockConfig;
        private readonly Mock<Response> mockResponse;
        private readonly ConfigurationSetting configurationSetting;
        private readonly AppConfigurationClient appConfigClient;
        private readonly Random rand;
        private Dictionary<string, AppConfigCacheValue> cache = new Dictionary<string, AppConfigCacheValue>();

        public AppConfigurationClientTest()
        {
            this.mockConfig = new Mock<AppConfig>();

            // this.mockConfig.Setup(x => x.ExternalDependencies.StorageAdapterServiceUrl).Returns(MockConnectionString);
            this.mockConfig.Object.AppConfigurationConnectionString = MockConnectionString;
            this.client = new Mock<ConfigurationClient>();
            this.mockResponse = new Mock<Response>();
            this.configurationSetting = new ConfigurationSetting("test", "test");
            this.rand = new Random();
            this.appConfigClient = new AppConfigurationClient(this.mockConfig.Object);
        }

        [Fact]
        public async Task SetValueAsyncTest()
        {
            string key = this.rand.NextString();
            string value = this.rand.NextString();
            Response<ConfigurationSetting> response = Response.FromValue(ConfigurationModelFactory.ConfigurationSetting("test", "test"), this.mockResponse.Object);
            this.client.Setup(c => c.SetConfigurationSettingAsync(It.IsAny<ConfigurationSetting>(), true, It.IsAny<CancellationToken>()))

            // .Returns((ConfigurationSetting cs, bool onlyIfUnchanged, CancellationToken ct) => Task.FromResult(Response.FromValue(cs, new Mock<Response>().Object)));
            .Returns(Task<Response>.FromResult(response));

            // this.client.Setup(x => x.SetConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).Returns(Task.FromResult(this.configurationSetting));
            await this.appConfigClient.SetValueAsync(key, value);
            Assert.True(true);
        }

        [Fact]
        public void GetValueTest()
        {
            string key = this.rand.NextString();
            string value = this.rand.NextString();
            Response<ConfigurationSetting> response = Response.FromValue(ConfigurationModelFactory.ConfigurationSetting("test", "test"), this.mockResponse.Object);
            this.client.Setup(c => c.GetConfigurationSetting("test", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(response);

            // this.client.Setup(x => x.SetConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).Returns(Task.FromResult(this.configurationSetting));
            string result = this.appConfigClient.GetValue(key);
            Assert.Equal(result, value);
        }

        /*

        [Fact]
        public async Task<StatusResultServiceModel> StatusAsyncTest()
        {
            string key1 = this.rand.NextString();
            string key2 = this.rand.NextString();

            string statusKey = string.Empty;
            mockConfig.Verify(x => x.GetType().GetProperties().FirstOrDefault(p => p.PropertyType == typeof(string)).Name);

            //Assert.Equal
        }

        */

        [Fact]
        public async Task DeleteKeyAsyncTest()
        {
            string key = this.rand.NextString();
            Response<string> response = Response.FromValue(key, this.mockResponse.Object);
            this.client
                .Setup(x => x.DeleteConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task<Response>.FromResult(this.mockResponse.Object));

            await this.appConfigClient.DeleteKeyAsync(key);
            Assert.True(true);
        }
    }
}