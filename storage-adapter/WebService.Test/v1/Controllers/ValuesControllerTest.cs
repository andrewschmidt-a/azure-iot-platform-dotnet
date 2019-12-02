// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Diagnostics;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.TestHelpers;
using Mmm.Platform.IoT.StorageAdapter.Services;
using Mmm.Platform.IoT.StorageAdapter.Services.Models;
using Mmm.Platform.IoT.StorageAdapter.WebService.v1.Controllers;
using Mmm.Platform.IoT.StorageAdapter.WebService.Wrappers;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Mmm.Platform.IoT.StorageAdapter.WebService.Test.v1.Controllers
{
    public class ValuesControllerTest
    {
        private readonly Mock<IKeyValueContainer> mockContainer;
        private readonly Mock<IKeyGenerator> mockGenerator;
        private readonly ValuesController controller;
        private readonly Random rand = new Random();

        public ValuesControllerTest()
        {
            this.mockContainer = new Mock<IKeyValueContainer>();
            this.mockGenerator = new Mock<IKeyGenerator>();

            this.controller = new ValuesController(
                this.mockContainer.Object,
                this.mockGenerator.Object,
                new Logger("UnitTest", LogLevel.Debug));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etag = this.rand.NextString();
            var timestamp = this.rand.NextDateTimeOffset();

            var model = new ValueServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = data,
                ETag = etag,
                Timestamp = timestamp
            };

            this.mockContainer
                .Setup(x => x.GetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(model);

            var result = await this.controller.Get(collectionId, key);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal("Value;1", result.Metadata["$type"]);
            Assert.Equal(timestamp.ToString(CultureInfo.InvariantCulture), result.Metadata["$modified"]);
            Assert.Equal($"/v1/collections/{collectionId}/values/{key}", result.Metadata["$uri"]);

            this.mockContainer
                .Verify(x => x.GetAsync(
                        It.Is<string>(s => s == collectionId),
                        It.Is<string>(s => s == key)),
                    Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllTest()
        {
            var collectionId = this.rand.NextString();

            var models = new[]
            {
                new ValueServiceModel
                {
                    CollectionId = collectionId,
                    Key = this.rand.NextString(),
                    Data = this.rand.NextString(),
                    ETag = this.rand.NextString(),
                    Timestamp = this.rand.NextDateTimeOffset()
                },
                new ValueServiceModel
                {
                    CollectionId = collectionId,
                    Key = this.rand.NextString(),
                    Data = this.rand.NextString(),
                    ETag = this.rand.NextString(),
                    Timestamp = this.rand.NextDateTimeOffset()
                },
                new ValueServiceModel
                {
                    CollectionId = collectionId,
                    Key = this.rand.NextString(),
                    Data = this.rand.NextString(),
                    ETag = this.rand.NextString(),
                    Timestamp = this.rand.NextDateTimeOffset()
                }
            };

            this.mockContainer
                .Setup(x => x.GetAllAsync(
                    It.IsAny<string>()))
                .ReturnsAsync(models);

            var result = await this.controller.Get(collectionId);

            var jsonResponse = JObject.FromObject(result);
            Assert.True(jsonResponse.TryGetValue("Items", out JToken value));

            Assert.Equal(result.Items.Count(), models.Length);
            foreach (var item in result.Items)
            {
                var model = models.Single(m => m.Key == item.Key);
                Assert.Equal(item.Data, model.Data);
                Assert.Equal(item.ETag, model.ETag);
                Assert.Equal("Value;1", item.Metadata["$type"]);
                Assert.Equal(model.Timestamp.ToString(CultureInfo.InvariantCulture), item.Metadata["$modified"]);
                Assert.Equal($"/v1/collections/{collectionId}/values/{model.Key}", item.Metadata["$uri"]);
            }

            Assert.Equal("ValueList;1", result.Metadata["$type"]);
            Assert.Equal($"/v1/collections/{collectionId}/values", result.Metadata["$uri"]);

            this.mockContainer
                .Verify(x => x.GetAllAsync(
                    It.Is<string>(s => s == collectionId)));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PostTest()
        {
            var collectionId = this.rand.NextString();
            var key = Guid.NewGuid().ToString();
            var data = this.rand.NextString();
            var etag = this.rand.NextString();
            var timestamp = this.rand.NextDateTimeOffset();

            var modelIn = new ValueServiceModel
            {
                Data = data
            };

            var modelOut = new ValueServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = data,
                ETag = etag,
                Timestamp = timestamp
            };

            this.mockGenerator
                .Setup(x => x.Generate())
                .Returns(key);

            this.mockContainer
                .Setup(x => x.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ValueServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await this.controller.Post(collectionId, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal("Value;1", result.Metadata["$type"]);
            Assert.Equal(modelOut.Timestamp.ToString(CultureInfo.InvariantCulture), result.Metadata["$modified"]);
            Assert.Equal($"/v1/collections/{collectionId}/values/{key}", result.Metadata["$uri"]);

            this.mockContainer
                .Verify(x => x.CreateAsync(
                    It.Is<string>(s => s == collectionId),
                    It.Is<string>(s => s == key),
                    It.Is<ValueServiceModel>(m => m.Equals(modelIn))));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PutNewTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etag = this.rand.NextString();
            var timestamp = this.rand.NextDateTimeOffset();

            var modelIn = new ValueServiceModel
            {
                Data = data
            };

            var modelOut = new ValueServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = data,
                ETag = etag,
                Timestamp = timestamp
            };

            this.mockContainer
                .Setup(x => x.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ValueServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await this.controller.Put(collectionId, key, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
            Assert.Equal("Value;1", result.Metadata["$type"]);
            Assert.Equal(modelOut.Timestamp.ToString(CultureInfo.InvariantCulture), result.Metadata["$modified"]);
            Assert.Equal($"/v1/collections/{collectionId}/values/{key}", result.Metadata["$uri"]);

            this.mockContainer
                .Verify(x => x.CreateAsync(
                        It.Is<string>(s => s == collectionId),
                        It.Is<string>(s => s == key),
                        It.Is<ValueServiceModel>(m => m.Equals(modelIn))),
                    Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PutUpdateTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();
            var data = this.rand.NextString();
            var etagOld = this.rand.NextString();
            var etagNew = this.rand.NextString();
            var timestamp = this.rand.NextDateTimeOffset();

            var modelIn = new ValueServiceModel
            {
                Data = data,
                ETag = etagOld
            };

            var modelOut = new ValueServiceModel
            {
                CollectionId = collectionId,
                Key = key,
                Data = data,
                ETag = etagNew,
                Timestamp = timestamp
            };

            this.mockContainer
                .Setup(x => x.UpsertAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ValueServiceModel>()))
                .ReturnsAsync(modelOut);

            var result = await this.controller.Put(collectionId, key, modelIn);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etagNew);
            Assert.Equal("Value;1", result.Metadata["$type"]);
            Assert.Equal(modelOut.Timestamp.ToString(CultureInfo.InvariantCulture), result.Metadata["$modified"]);
            Assert.Equal($"/v1/collections/{collectionId}/values/{key}", result.Metadata["$uri"]);

            this.mockContainer
                .Verify(x => x.UpsertAsync(
                        It.Is<string>(s => s == collectionId),
                        It.Is<string>(s => s == key),
                        It.Is<ValueServiceModel>(m => m.Equals(modelIn))),
                    Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task DeleteTest()
        {
            var collectionId = this.rand.NextString();
            var key = this.rand.NextString();

            this.mockContainer
                .Setup(x => x.DeleteAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(0));

            await this.controller.Delete(collectionId, key);

            this.mockContainer
                .Verify(x => x.DeleteAsync(
                        It.Is<string>(s => s == collectionId),
                        It.Is<string>(s => s == key)),
                    Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task ValidateKeyTest()
        {
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await this.controller.Delete("collection", "*"));

            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await this.controller.Delete("collection", new string('a', 256)));
        }
    }
}
