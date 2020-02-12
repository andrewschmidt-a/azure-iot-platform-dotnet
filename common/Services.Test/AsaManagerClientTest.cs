// <copyright file="AsaManagerClientTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Iot.Common.Services.Config;
using Mmm.Iot.Common.Services.External.AsaManager;
using Mmm.Iot.Common.Services.Helpers;
using Mmm.Iot.Common.TestHelpers;
using Moq;
using Xunit;

namespace Mmm.Iot.Common.Services.Test
{
    public class AsaManagerClientTest
    {
        private const string MockServiceUri = @"http://mockserviceuri";

        private readonly MockExternalClientHelper mockExternalClientHelper;
        private readonly Mock<AppConfig> mockConfig;
        private readonly AsaManagerClient client;
        private readonly Random rand;

        public AsaManagerClientTest()
        {
            this.mockConfig = new Mock<AppConfig>();
            this.mockConfig
                .Setup(x => x.ExternalDependencies.AsaManagerServiceUrl)
                .Returns(MockServiceUri);

            this.mockExternalClientHelper = new MockExternalClientHelper();
            this.client = new AsaManagerClient(this.mockConfig.Object, this.mockExternalClientHelper.ExternalRequestHelper);
            this.rand = new Random();
        }

        [Fact]
        public async Task BeginRulesConversionAsyncTest()
        {
            var model = new BeginConversionApiModel
            {
                TenantId = this.rand.NextString(),
                OperationId = this.rand.NextString(),
            };

            this.MockProcessRequestAsync(model);

            var response = await this.client.BeginDeviceGroupsConversionAsync();

            this.VerifyProcessRequestAsync<BeginConversionApiModel>("rules");

            Assert.Equal(model.TenantId, response.TenantId);
            Assert.Equal(model.OperationId, response.OperationId);
        }

        [Fact]
        public async Task BeginDeviceGroupsConversionAsyncTest()
        {
            var model = new BeginConversionApiModel
            {
                TenantId = this.rand.NextString(),
                OperationId = this.rand.NextString(),
            };

            this.MockProcessRequestAsync(model);

            var response = await this.client.BeginDeviceGroupsConversionAsync();

            this.VerifyProcessRequestAsync<BeginConversionApiModel>("devicegroups");

            Assert.Equal(model.TenantId, response.TenantId);
            Assert.Equal(model.OperationId, response.OperationId);
        }

        private void MockProcessRequestAsync<T>(T responseModel)
        {
            this.mockExternalClientHelper.MockExternalRequestHelper
                .Setup(x => x.ProcessRequestAsync<T>(
                    It.Is<HttpMethod>(m => m == HttpMethod.Post),
                    It.IsAny<string>(),
                    null))
                .ReturnsAsync(responseModel);
        }

        private void VerifyProcessRequestAsync<T>(string entity)
        {
            this.mockExternalClientHelper.MockExternalRequestHelper
                .Verify(
                    x => x.ProcessRequestAsync<T>(
                        It.Is<HttpMethod>(m => m == HttpMethod.Post),
                        It.Is<string>(s => s == $"{MockServiceUri}/{entity}"),
                        null),
                    Times.Once);
        }
    }
}