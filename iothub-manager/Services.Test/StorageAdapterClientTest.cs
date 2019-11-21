// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.IoTHubManager.Services.Runtime;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Xunit;
using HttpResponse = Mmm.Platform.IoT.Common.Services.Http.HttpResponse;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Test
{
    public class StorageAdapterClientTest
    {
        private const string MOCK_SERVICE_URI = @"http://mockstorageadapter";
        private const string AZDS_ROUTE_KEY = "azds-route-as";

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly StorageAdapterClient client;
        private readonly Random rand;
        private readonly Mock<IHttpContextAccessor> httpContextAccessor;

        public StorageAdapterClientTest()
        {
            this.mockHttpClient = new Mock<IHttpClient>();
            this.httpContextAccessor = new Mock<IHttpContextAccessor>();
            this.httpContextAccessor.Setup(t => t.HttpContext.Request.HttpContext.Items).Returns(new Dictionary<object, object>()
                {{"TenantID", "test_tenant"}});
            this.httpContextAccessor.Setup(t => t.HttpContext.Request.Headers).Returns(new HeaderDictionary() { { AZDS_ROUTE_KEY, "mockDevSpace" } });
            this.client = new StorageAdapterClient(
                this.mockHttpClient.Object,
                new ServicesConfig
                {
                    StorageAdapterApiUrl = MOCK_SERVICE_URI
                },
                new Mock<ILogger<StorageAdapterClient>>().Object,
                this.httpContextAccessor.Object);
            this.rand = new Random();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAsyncTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etag = this.rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(new ValueApiModel
                {
                    Key = key,
                    Data = data,
                    ETag = etag
                })
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await this.client.GetAsync(collectionId, key);

            this.mockHttpClient
                .Verify(x => x.GetAsync(
                        It.Is<IHttpRequest>(r => r.Check($"{MOCK_SERVICE_URI}/collections/{collectionId}/values/{key}"))),
                    Times.Once);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAsyncNotFoundTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccessStatusCode = false
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            await Assert.ThrowsAsync<ResourceNotFoundException>(async () =>
                await this.client.GetAsync(collectionId, key));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UpdateAsyncTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etagOld = this.rand.NextString();
            var etagNew = this.rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(new ValueApiModel
                {
                    Key = key,
                    Data = data,
                    ETag = etagNew
                })
            };

            this.mockHttpClient
                .Setup(x => x.PutAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await this.client.UpdateAsync(collectionId, key, data, etagOld);

            this.mockHttpClient
                .Verify(x => x.PutAsync(
                        It.Is<IHttpRequest>(r => r.Check<ValueApiModel>($"{MOCK_SERVICE_URI}/collections/{collectionId}/values/{key}", m => m.Data == data && m.ETag == etagOld))),
                    Times.Once);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etagNew);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UpdateAsyncConflictTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etag = this.rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                IsSuccessStatusCode = false
            };

            this.mockHttpClient
                .Setup(x => x.PutAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            await Assert.ThrowsAsync<ConflictingResourceException>(async () =>
                await this.client.UpdateAsync(collectionId, key, data, etag));
        }
    }
}
