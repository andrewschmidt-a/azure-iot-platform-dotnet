
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.AsaManager.Services.Exceptions;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.Models;
using Mmm.Platform.IoT.AsaManager.Services.Models.Rules;
using Mmm.Platform.IoT.AsaManager.Services.Test.Helpers;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Mmm.Platform.IoT.AsaManager.Services.Test
{
    public class RulesConverterTest 
    {
        private Mock<IBlobStorageClient> mockBlobStorageClient;
        private Mock<IStorageAdapterClient> mockStorageAdapterClient;
        private Mock<ILogger<RulesConverter>> mockLog;
        private RulesConverter converter;
        private readonly Random rand;
        private CreateEntityHelper entityHelper;

        public RulesConverterTest()
        {
            this.mockBlobStorageClient = new Mock<IBlobStorageClient> ();
            this.mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            this.mockLog = new Mock<ILogger<RulesConverter>>();
            this.rand = new Random();
            this.entityHelper = new CreateEntityHelper(this.rand);

            this.converter = new RulesConverter(
                this.mockBlobStorageClient.Object,
                this.mockStorageAdapterClient.Object,
                this.mockLog.Object);
        }

        [Fact]
        public async Task ConvertAsyncReturnsExpectedModel()
        {
            string tenantId = this.rand.NextString();
            List<ValueApiModel> rulesList = new List<ValueApiModel>
            {
                this.entityHelper.CreateRule(),
                this.entityHelper.CreateRule()
            };
            ValueListApiModel rules = new ValueListApiModel
            {
                Items = rulesList
            };

            this.mockStorageAdapterClient
                .Setup(c => c.GetAllAsync(
                    It.Is<String>(s => s == this.converter.Entity)))
                .ReturnsAsync(rules);
            
            this.mockBlobStorageClient
                .Setup(c => c.CreateBlobAsync(
                    It.IsAny<String>(),
                    It.IsAny<String>(),
                    It.IsAny<String>()))
                .Returns(Task.CompletedTask);

            ConversionApiModel conversionResponse = await this.converter.ConvertAsync(tenantId);

            this.mockStorageAdapterClient
                .Verify(c => c.GetAllAsync(
                        It.Is<String>(s => s == this.converter.Entity)),
                    Times.Once);
            this.mockBlobStorageClient
                .Verify(c => c.CreateBlobAsync(
                        It.IsAny<String>(),
                        It.IsAny<String>(),
                        It.IsAny<String>()),
                    Times.Once);

            Assert.Equal(conversionResponse.Entities, rules);
            Assert.Equal(conversionResponse.TenantId, tenantId);
        }

        [Fact]
        public async Task ConvertAsyncThrowsOnEmptyRules()
        {
            string tenantId = this.rand.NextString();
            ValueListApiModel rules = new ValueListApiModel
            {
                Items = new List<ValueApiModel>()
            };

            this.mockStorageAdapterClient
                .Setup(c => c.GetAllAsync(
                    It.Is<String>(s => s == this.converter.Entity)))
                .ReturnsAsync(rules);

            Func<Task> conversion = async () => await this.converter.ConvertAsync(tenantId);

            await Assert.ThrowsAsync<EmptyEntitesException>(conversion);
        }
    }
}