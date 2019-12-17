// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Xunit;
using HttpResponse = Mmm.Platform.IoT.Common.Services.Http.HttpResponse;

namespace Mmm.Platform.IoT.Common.Services.Test
{
    public class UserManagementClientTest
    {
        private const string MOCK_SERVICE_URI = @"http://mockauth";
        private const string AZDS_ROUTE_KEY = "azds-route-as";

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private readonly Mock<ExternalRequestHelper> mockRequestHelper;
        private readonly Mock<IUserManagementClientConfig> mockConfig;
        private readonly UserManagementClient client;
        private readonly Random rand;

        public UserManagementClientTest()
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

            this.mockConfig = new Mock<IUserManagementClientConfig>();
            this.mockConfig
                .Setup(x => x.UserManagementApiUrl)
                .Returns(MOCK_SERVICE_URI);

            this.client = new UserManagementClient(
                this.mockConfig.Object,
                this.mockRequestHelper.Object);
            this.rand = new Random();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllowedActions_ReturnValues()
        {
            var userObjectId = this.rand.NextString();
            var roles = new List<string> { "Admin" };
            var allowedActions = new List<string> { "CreateDeviceGroups", "UpdateDeviceGroups" };

            var method = HttpMethod.Post;
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(allowedActions)
            };

            this.mockHttpClient
                .Setup(x => x.SendAsync(
                    It.IsAny<IHttpRequest>(),
                    It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            var result = await this.client.GetAllowedActionsAsync(userObjectId, roles);
            this.mockHttpClient
                .Verify(x => x.SendAsync(
                    It.Is<IHttpRequest>(r => r.Check($"{MOCK_SERVICE_URI}/users/{userObjectId}/allowedActions")),
                    It.Is<HttpMethod>(m => m == method)),
                Times.Once);

            Assert.Equal(allowedActions, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllowedActions_ReturnNotFound()
        {
            var userObjectId = this.rand.NextString();
            var roles = new List<string> { "Unknown" };

            var method = HttpMethod.Post;
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccessStatusCode = false
            };

            this.mockHttpClient
                .Setup(x => x.SendAsync(
                    It.IsAny<IHttpRequest>(),
                    It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            await Assert.ThrowsAsync<ResourceNotFoundException>(async () =>
                await this.client.GetAllowedActionsAsync(userObjectId, roles));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllowedActions_ReturnError()
        {
            var userObjectId = this.rand.NextString();
            var roles = new List<string> { "Unknown" };

            var method = HttpMethod.Post;
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccessStatusCode = false
            };

            this.mockHttpClient
                .Setup(x => x.SendAsync(
                    It.IsAny<IHttpRequest>(),
                    It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await this.client.GetAllowedActionsAsync(userObjectId, roles));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetToken_ReturnsValue()
        {
            // Arrange
            var token = new TokenApiModel()
            {
                AccessToken = "1234ExampleToken",
                AccessTokenType = "Bearer",
                Audience = "https://management.azure.com/",
                Authority = "https://login.microsoftonline.com/12345/"
            };

            var method = HttpMethod.Get;
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(token)
            };

            this.mockHttpClient
                .Setup(x => x.SendAsync(
                    It.IsAny<IHttpRequest>(),
                    It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            // Act
            var result = await this.client.GetTokenAsync();

            // Assert
            this.mockHttpClient
                .Verify(x => x.SendAsync(
                    It.Is<IHttpRequest>(r => r.Check($"{MOCK_SERVICE_URI}/users/default/token")),
                    It.Is<HttpMethod>(m => m == method)),
                Times.Once);

            Assert.Equal(token.AccessToken, result);
        }
    }
}