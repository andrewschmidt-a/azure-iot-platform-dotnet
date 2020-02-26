// <copyright file="HttpClientTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Iot.Common.Services.Http;
using Mmm.Iot.Common.Services.Models;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;
using HttpClient = Mmm.Iot.Common.Services.Http.HttpClient;
using HttpResponse = Mmm.Iot.Common.Services.Http.HttpResponse;

namespace Mmm.Iot.Common.Services.Test
{
    public class HttpClientTest
    {
        private const string MockServiceUri = @"http://mockclient";
        private const string AzdsRouteKey = "azds-route-as";
        private readonly Mock<ILogger<HttpClient>> mockLogger;

        // private readonly Mock<HttpClientHandler> mockClientHandler;
        // private System.Net.Http.HttpClient client;
        // private HttpClient httpClient;

        // private readonly Mock<System.Net.Http.IHttpClientFactory> mockFactory;
        public HttpClientTest()
        {
            this.mockLogger = new Mock<ILogger<HttpClient>>();

            // this.mockClientHandler = new Mock<HttpClientHandler>();
            // this.client = new Mock<System.Net.Http.HttpClient>(this.mockClientHandler.Object);
            /*
            this.mockFactory = new Mock<System.Net.Http.IHttpClientFactory>();
            this.mockFactory
                .Setup(x => x.CreateClient())
                .Returns(this.client.Object);
            */

            // this.httpClient = new HttpClient(this.mockLogger.Object);
        }

        /*
        [Fact]
        public async Task GetAsyncTest()
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new System.Uri(MockServiceUri),
            };

            var healthyStatus = new StatusResultServiceModel(true, "all good");

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(healthyStatus)),
                RequestMessage = httpRequest,
                ReasonPhrase = string.Empty,
                Version = new System.Version("1.0"),
            };

            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var result = await this.httpclient.GetAsync(It.IsAny<IHttpRequest>());
            Assert.Equal(result.StatusCode, response.StatusCode);
        }
        */
        /*

        [Fact]
        public async Task PostAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
            };

            this.client
                .Setup(
                    x => x.SendAsync(
                        It.IsAny<IHttpRequest>(),
                        It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            var result = await this.httpclient.PostAsync(It.IsAny<IHttpRequest>());
            Assert.Equal(result.StatusCode, response.StatusCode);
        }

        [Fact]
        public async Task PutAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
            };

            this.client
                .Setup(
                    x => x.SendAsync(
                        It.IsAny<IHttpRequest>(),
                        It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            var result = await this.httpclient.PutAsync(It.IsAny<IHttpRequest>());
            Assert.Equal(result.StatusCode, response.StatusCode);
        }

        [Fact]
        public async Task PatchAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
            };

            this.client
                .Setup(
                    x => x.SendAsync(
                        It.IsAny<IHttpRequest>(),
                        It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            var result = await this.httpclient.PatchAsync(It.IsAny<IHttpRequest>());
            Assert.Equal(result.StatusCode, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
            };

            this.client
                .Setup(
                    x => x.SendAsync(
                        It.IsAny<IHttpRequest>(),
                        It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            var result = await this.httpclient.DeleteAsync(It.IsAny<IHttpRequest>());
            Assert.Equal(result.StatusCode, response.StatusCode);
        }

        [Fact]
        public async Task HeadAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
            };

            this.client
                .Setup(
                    x => x.SendAsync(
                        It.IsAny<IHttpRequest>(),
                        It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            var result = await this.httpclient.HeadAsync(It.IsAny<IHttpRequest>());
            Assert.Equal(result.StatusCode, response.StatusCode);
        }

        [Fact]
        public async Task OptionsAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
            };

            this.client
                .Setup(
                    x => x.SendAsync(
                        It.IsAny<IHttpRequest>(),
                        It.IsAny<HttpMethod>()))
                .ReturnsAsync(response);

            var result = await this.httpclient.OptionsAsync(It.IsAny<IHttpRequest>());
            Assert.Equal(result.StatusCode, response.StatusCode);
        }

    */
        [Fact]
        public async Task SendAsyncTest()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("[{'id':1,'value':'1'}]"),
               })
                .Verifiable();

            var client = new Mock<System.Net.Http.HttpClient>(handlerMock.Object);

            var httpClient = new HttpClient(this.mockLogger.Object, client.Object);

            var result = await httpClient.SendAsync(It.IsAny<IHttpRequest>(), It.IsAny<HttpMethod>());
            Assert.True(result.IsSuccess);
        }
    }
}