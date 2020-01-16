// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Xunit;
using HttpResponse = Mmm.Platform.IoT.Common.Services.Http.HttpResponse;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services.Test
{
    public class ExternalServiceClientTest
    {
        private const string MOCK_SERVICE_URI = @"http://mockclient";
        private const string AZDS_ROUTE_KEY = "azds-route-as";

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private readonly Mock<ExternalRequestHelper> mockRequestHelper;

        private readonly IExternalServiceClient client;

        public ExternalServiceClientTest()
        {
            this.mockHttpClient = new Mock<IHttpClient>();
            this.mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            this.mockHttpContextAccessor
                .Setup(t => t.HttpContext.Request.HttpContext.Items)
                .Returns(new Dictionary<object, object>(){{"TenantID", "test_tenant"}});
            this.mockHttpContextAccessor
                .Setup(t => t.HttpContext.Request.Headers)
                .Returns(new HeaderDictionary() { { AZDS_ROUTE_KEY, "mockDevSpace" } });
            this.mockRequestHelper = new Mock<ExternalRequestHelper>(
                this.mockHttpClient.Object,
                this.mockHttpContextAccessor.Object);
            
            this.client = new ExternalServiceClient(MOCK_SERVICE_URI, this.mockRequestHelper.Object);
        }

        [Fact]
        public async Task GetHealthyStatusAsyncTest()
        {
            var healthyStatus = new StatusResultServiceModel(true, "all good");
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(healthyStatus)
            };

            this.mockHttpClient
                .Setup(x => x.SendAsync(
                    It.IsAny<IHttpRequest>(),
                    It.Is<HttpMethod>(method => method == HttpMethod.Get)))
                .ReturnsAsync(response);
            
            var result = await this.client.StatusAsync();

            this.mockHttpClient
                .Verify(x => x.SendAsync(
                        It.Is<IHttpRequest>(r => r.Check($"{MOCK_SERVICE_URI}/status")),
                        It.Is<HttpMethod>(method => method == HttpMethod.Get)),
                    Times.Once);

            Assert.Equal(result.IsHealthy, healthyStatus.IsHealthy);
            Assert.Equal(result.Message, healthyStatus.Message);
        }

        [Fact]
        public async Task StatusAsyncReturnsUnhealthyOnExceptionTest()
        {
            this.mockHttpClient
                .Setup(x => x.SendAsync(
                    It.IsAny<IHttpRequest>(),
                    It.Is<HttpMethod>(method => method == HttpMethod.Get)))
                .ThrowsAsync(new Exception());
            
            var response = await this.client.StatusAsync();

            Assert.False(response.IsHealthy);
        }
    }
}