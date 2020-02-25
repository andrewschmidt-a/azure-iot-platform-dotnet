// <copyright file="StreamAnalyticsHelperTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.StreamAnalytics;
using Microsoft.Azure.Management.StreamAnalytics.Models;
using Microsoft.Extensions.Logging;
using Mmm.Iot.Common.Services.Config;
using Mmm.Iot.Common.Services.Helpers;
using Mmm.Iot.Common.TestHelpers;
using Mmm.Iot.TenantManager.Services.Helpers;
using Mmm.Iot.TenantManager.Services.Models;
using Moq;
using TestStack.Dossier;
using TestStack.Dossier.EquivalenceClasses;
using Xunit;

namespace Mmm.Iot.TenantManager.Services.Test
{
    public class StreamAnalyticsHelperTest
    {
        private const string MockSubscriptionId = "mocksub";
        private const string MockResourceGroup = "mockrg";
        private const string MockJobName = "mockjob";

        private readonly Mock<AppConfig> mockAppConfig;
        private readonly Mock<GlobalConfig> mockGlobalConfig;
        private readonly Mock<ITokenHelper> mockTokenHelper;
        private readonly Mock<StreamAnalyticsHelper> mockHelper;
        private readonly Mock<IStreamAnalyticsManagementClient> mockClient;
        private readonly Mock<IStreamingJobsOperations> mockStreamingJobs;
        private readonly IStreamAnalyticsHelper helper;

        private Random random = new Random();

        public StreamAnalyticsHelperTest()
        {
            this.random = new Random();

            this.mockGlobalConfig = new Mock<GlobalConfig>();
            this.mockGlobalConfig
                .Setup(x => x.ResourceGroup)
                .Returns(MockResourceGroup);

            this.mockAppConfig = new Mock<AppConfig>();
            this.mockAppConfig
                .Setup(x => x.Global)
                .Returns(this.mockGlobalConfig.Object);

            this.mockTokenHelper = new Mock<ITokenHelper>();

            this.mockStreamingJobs = new Mock<IStreamingJobsOperations>();
            this.mockClient = new Mock<IStreamAnalyticsManagementClient>();
            this.mockClient
                .Setup(x => x.StreamingJobs)
                .Returns(this.mockStreamingJobs.Object);

            this.mockHelper = new Mock<StreamAnalyticsHelper>(this.mockAppConfig.Object, this.mockTokenHelper.Object);
            this.mockHelper
                .Setup(x => x.GetClientAsync())
                .ReturnsAsync(this.mockClient.Object);

            this.helper = this.mockHelper.Object;
        }

        [Fact]
        public async Task StartAsyncCallsClientBeginStartAsyncTest()
        {
            var jobName = this.random.NextString();

            this.mockStreamingJobs
                .Setup(x => x.BeginStartAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<StartStreamingJobParameters>(),
                    It.IsAny<CancellationToken>()))
                .Verifiable();

            await this.helper.StartAsync(jobName);

            this.mockStreamingJobs
                .Verify(
                    x => x.BeginStartAsync(
                        It.Is<string>(s => s == MockResourceGroup),
                        It.Is<string>(s => s == jobName),
                        It.IsAny<StartStreamingJobParameters>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task StopAsyncCallsClientBeginStopAsyncTest()
        {
            var jobName = this.random.NextString();

            this.mockStreamingJobs
                .Setup(x => x.BeginStopAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Verifiable();

            await this.helper.StopAsync(jobName);

            this.mockStreamingJobs
                .Verify(
                    x => x.BeginStopAsync(
                        It.Is<string>(s => s == MockResourceGroup),
                        It.Is<string>(s => s == jobName),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}