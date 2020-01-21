// <copyright file="MessagesTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Test
{
    public class MessagesTest
    {
        private const int SKIP = 0;
        private const int LIMIT = 1000;

        private readonly Mock<IStorageClient> storageClient;
        private readonly Mock<ITimeSeriesClient> timeSeriesClient;
        private readonly Mock<ILogger<Messages>> logger;
        private readonly Mock<IHttpContextAccessor> httpContextAccessor;
        private readonly Mock<IAppConfigurationHelper> appConfigHelper;
        private readonly IMessages messages;

        public MessagesTest()
        {
            var servicesConfig = new AppConfig()
            {
                DeviceTelemetryService = new DeviceTelemetryServiceConfig
                {
                    Messages = new MessagesConfig
                    {
                        Database = "database",
                        TelemetryStorageType = "tsi",
                    },
                },
            };
            storageClient = new Mock<IStorageClient>();
            timeSeriesClient = new Mock<ITimeSeriesClient>();
            httpContextAccessor = new Mock<IHttpContextAccessor>();
            appConfigHelper = new Mock<IAppConfigurationHelper>();
            logger = new Mock<ILogger<Messages>>();
            messages = new Messages(
                servicesConfig,
                storageClient.Object,
                timeSeriesClient.Object,
                logger.Object,
                httpContextAccessor.Object,
                appConfigHelper.Object);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task InitialListIsEmptyAsync()
        {
            // Arrange
            ThereAreNoMessagesInStorage();
            var devices = new string[] { "device1" };

            // Act
            var list = await messages.ListAsync(null, null, "asc", SKIP, LIMIT, devices);

            // Assert
            Assert.Empty(list.Messages);
            Assert.Empty(list.Properties);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetListWithValuesAsync()
        {
            // Arrange
            ThereAreSomeMessagesInStorage();
            var devices = new string[] { "device1" };

            // Act
            var list = await messages.ListAsync(null, null, "asc", SKIP, LIMIT, devices);

            // Assert
            Assert.NotEmpty(list.Messages);
            Assert.NotEmpty(list.Properties);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task ThrowsOnInvalidInput()
        {
            // Arrange
            var xssString = "<body onload=alert('test1')>";
            var xssList = new List<string>
            {
                "<body onload=alert('test1')>",
                "<IMG SRC=j&#X41vascript:alert('test2')>",
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidInputException>(async () => await messages.ListAsync(null, null, xssString, 0, LIMIT, xssList.ToArray()));
        }

        private void ThereAreNoMessagesInStorage()
        {
            timeSeriesClient.Setup(x => x.QueryEventsAsync(null, null, It.IsAny<string>(), SKIP, LIMIT, It.IsAny<string[]>()))
                .ReturnsAsync(new MessageList());
        }

        private void ThereAreSomeMessagesInStorage()
        {
            var sampleMessages = new List<Message>();
            var sampleProperties = new List<string>();

            var data = new JObject
            {
                { "data.sample_unit", "mph" },
                { "data.sample_speed", "10" },
            };

            sampleMessages.Add(new Message("id1", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), data));
            sampleMessages.Add(new Message("id2", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), data));

            sampleProperties.Add("data.sample_unit");
            sampleProperties.Add("data.sample_speed");

            timeSeriesClient.Setup(x => x.QueryEventsAsync(null, null, It.IsAny<string>(), SKIP, LIMIT, It.IsAny<string[]>()))
                .ReturnsAsync(new MessageList(sampleMessages, sampleProperties));
        }
    }
}