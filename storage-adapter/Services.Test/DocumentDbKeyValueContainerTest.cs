// <copyright file="DocumentDbKeyValueContainerTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.AppConfiguration;
using Mmm.Platform.IoT.Common.TestHelpers;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;
using Moq;
using Xunit;

namespace Mmm.Platform.IoT.StorageAdapter.Services.Test
{
    public class DocumentDbKeyValueContainerTest
    {
        private const string MockTenantId = "mocktenant";
        private const string MockDatabaseId = "pcs-storage";
        private const string MockCollectionId = "mockcoll";
        private const string AppConfigConnString = "";
        private static readonly string MockCollectionLink = $"/dbs/{MockDatabaseId}/colls/{MockCollectionId}";
        private readonly Mock<IDocumentClient> mockClient;
        private readonly Mock<IHttpContextAccessor> mockContextAccessor;
        private readonly Mock<DocumentDbKeyValueContainer> mockContainer;
        private readonly DocumentDbKeyValueContainer container;
        private readonly Random rand = new Random();

        public DocumentDbKeyValueContainerTest()
        {
            this.mockContextAccessor = new Mock<IHttpContextAccessor>();
            DefaultHttpContext context = new DefaultHttpContext();
            context.Items.Add("TenantID", MockTenantId);
            this.mockContextAccessor.Setup(t => t.HttpContext).Returns(context);

            this.mockClient = new Mock<IDocumentClient>();
            var database = new Mock<ResourceResponse<Database>>();
            this.mockClient.Setup(t => t.ReadDatabaseAsync(It.IsAny<Uri>(), It.IsAny<RequestOptions>()))
                .Returns(Task.FromResult<ResourceResponse<Database>>(new Mock<ResourceResponse<Database>>().Object));
            this.mockClient.Setup(t => t.ReadDocumentCollectionAsync(It.IsAny<Uri>(), It.IsAny<RequestOptions>()))
                .Returns(Task.FromResult(new Mock<ResourceResponse<DocumentCollection>>().Object));

            // mock a specific tenant
            Mock<IAppConfigurationClient> mockAppConfigHelper = new Mock<IAppConfigurationClient>();

            // Mock service returns dummy data
            var config = new AppConfig();
            this.mockContainer = new Mock<DocumentDbKeyValueContainer>(
                new MockFactory<IDocumentClient>(this.mockClient),
                new MockExceptionChecker(),
                config,
                mockAppConfigHelper.Object,
                new Mock<ILogger<DocumentDbKeyValueContainer>>().Object,
                this.mockContextAccessor.Object);
            config.StorageAdapterService = new StorageAdapterServiceConfig { DocumentDbRus = 400 };
            mockAppConfigHelper.Setup(m => m.GetValue(It.IsAny<string>())).Returns(MockCollectionId);
            this.mockContainer.Setup(t => t.DocumentDbDatabaseId)
                .Returns(MockDatabaseId);
            this.mockContainer.Setup(t => t.DocumentDbCollectionId)
                .Returns(MockCollectionId);

            this.container = this.mockContainer.Object;
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAsyncTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etag = this.rand.NextString();
            var timestamp = this.rand.NextDateTimeOffset();

            var document = new Document();
            document.SetPropertyValue("CollectionId", collectionId);
            document.SetPropertyValue("Key", key);
            document.SetPropertyValue("Data", data);
            document.SetETag(etag);
            document.SetTimestamp(timestamp);
            var response = new ResourceResponse<Document>(document);

            this.mockClient
                .Setup(x => x.ReadDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await this.container.GetAsync(collectionId, key);

            Assert.Equal(result.CollectionId, collectionId);
            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.Timestamp, timestamp);

            this.mockClient
                .Verify(
                    x => x.ReadDocumentAsync(
                        It.Is<string>(s => s == $"{MockCollectionLink}/docs/{collectionId.ToLowerInvariant()}.{key.ToLowerInvariant()}"),
                        It.IsAny<RequestOptions>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAsyncNotFoundTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();

            this.mockClient
                .Setup(x => x.ReadDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceNotFoundException());

            await Assert.ThrowsAsync<ResourceNotFoundException>(async () =>
                await this.container.GetAsync(collectionId, key));
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAllAsyncTest()
        {
            var collectionId = this.rand.NextString();
            var documents = new[]
            {
                new KeyValueDocument(collectionId, this.rand.NextString(), this.rand.NextString()),
                new KeyValueDocument(collectionId, this.rand.NextString(), this.rand.NextString()),
                new KeyValueDocument(collectionId, this.rand.NextString(), this.rand.NextString()),
            };
            foreach (var doc in documents)
            {
                doc.SetETag(this.rand.NextString());
                doc.SetTimestamp(this.rand.NextDateTimeOffset());
            }

            this.mockClient
                .Setup(x => x.CreateDocumentQuery<KeyValueDocument>(
                    It.IsAny<string>(),
                    It.IsAny<FeedOptions>()))
                .Returns(documents.AsQueryable().OrderBy(doc => doc.Id));

            var result = (await this.container.GetAllAsync(collectionId)).ToList();

            Assert.Equal(result.Count(), documents.Length);
            foreach (var model in result)
            {
                var doc = documents.Single(d => d.Key == model.Key);
                Assert.Equal(model.CollectionId, collectionId);
                Assert.Equal(model.Data, doc.Data);
                Assert.Equal(model.ETag, doc.ETag);
                Assert.Equal(model.Timestamp, doc.Timestamp);
            }

            this.mockClient
                .Verify(
                    x => x.CreateDocumentQuery<KeyValueDocument>(
                        It.Is<string>(s => s == MockCollectionLink),
                        It.IsAny<FeedOptions>()),
                    Times.Once);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task CreateAsyncTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etag = this.rand.NextString();
            var timestamp = this.rand.NextDateTimeOffset();

            var document = new Document();
            document.SetPropertyValue("CollectionId", collectionId);
            document.SetPropertyValue("Key", key);
            document.SetPropertyValue("Data", data);
            document.SetETag(etag);
            document.SetTimestamp(timestamp);
            var response = new ResourceResponse<Document>(document);

            this.mockClient
                .Setup(x => x.CreateDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await this.container.CreateAsync(collectionId, key, new ValueServiceModel
            {
                Data = data,
            });

            Assert.Equal(result.CollectionId, collectionId);
            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal(result.Timestamp, timestamp);

            this.mockClient
                .Verify(
                    x => x.CreateDocumentAsync(
                        It.Is<string>(s => s == MockCollectionLink),
                        It.Is<KeyValueDocument>(doc => doc.Id == $"{collectionId.ToLowerInvariant()}.{key.ToLowerInvariant()}" && doc.CollectionId == collectionId && doc.Key == key && doc.Data == data),
                        It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task CreateAsyncConflictTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();

            this.mockClient
                .Setup(x => x.CreateDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConflictingResourceException());

            await Assert.ThrowsAsync<ConflictingResourceException>(async () =>
                await this.container.CreateAsync(collectionId, key, new ValueServiceModel
                {
                    Data = data,
                }));
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task UpsertAsyncTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etagOld = this.rand.NextString();
            var etagNew = this.rand.NextString();
            var timestamp = this.rand.NextDateTimeOffset();

            var document = new Document();
            document.SetPropertyValue("CollectionId", collectionId);
            document.SetPropertyValue("Key", key);
            document.SetPropertyValue("Data", data);
            document.SetETag(etagNew);
            document.SetTimestamp(timestamp);
            var response = new ResourceResponse<Document>(document);

            this.mockClient
                .Setup(x => x.UpsertDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await this.container.UpsertAsync(collectionId, key, new ValueServiceModel
            {
                Data = data,
                ETag = etagOld,
            });

            Assert.Equal(result.CollectionId, collectionId);
            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etagNew);
            Assert.Equal(result.Timestamp, timestamp);

            this.mockClient
                .Verify(
                    x => x.UpsertDocumentAsync(
                        It.Is<string>(s => s == MockCollectionLink),
                        It.Is<KeyValueDocument>(doc => doc.Id == $"{collectionId.ToLowerInvariant()}.{key.ToLowerInvariant()}" && doc.CollectionId == collectionId && doc.Key == key && doc.Data == data),
                        It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task UpsertAsyncConflictTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etag = this.rand.NextString();

            this.mockClient
                .Setup(x => x.UpsertDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConflictingResourceException());

            await Assert.ThrowsAsync<ConflictingResourceException>(async () =>
                await this.container.UpsertAsync(collectionId, key, new ValueServiceModel
                {
                    Data = data,
                    ETag = etag,
                }));
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task DeleteAsyncTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();

            this.mockClient
                .Setup(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResourceResponse<Document>)null);

            await this.container.DeleteAsync(collectionId, key);

            this.mockClient
                .Verify(
                    x => x.DeleteDocumentAsync(
                        It.Is<string>(s => s == $"{MockCollectionLink}/docs/{collectionId.ToLowerInvariant()}.{key.ToLowerInvariant()}"),
                        It.IsAny<RequestOptions>(),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task DeleteAsyncNotFoundTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();

            this.mockClient
                .Setup(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceNotFoundException());

            await this.container.DeleteAsync(collectionId, key);
        }
    }
}