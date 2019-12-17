
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.AsaManager.Services.Exceptions;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.Models;
using Mmm.Platform.IoT.AsaManager.Services.Models.Rules;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Services.Test
{
    public class RulesConverterTest 
    {
        private Mock<IBlobStorageClient> mockBlobStorageClient;
        private Mock<IStorageAdapterClient> mockStorageAdapterClient;
        private Mock<ILogger<RulesConverter>> mockLog;
        private RulesConverter converter;
        private readonly Random rand;

        public RulesConverterTest () {
            this.mockBlobStorageClient = new Mock<IBlobStorageClient> ();
            this.mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            this.mockLog = new Mock<ILogger<RulesConverter>>();
            this.rand = new Random();

            this.converter = new RulesConverter(
                this.mockBlobStorageClient.Object,
                this.mockStorageAdapterClient.Object,
                this.mockLog.Object);
        }

        public ValueApiModel CreateRule()
        {
            RuleDataModel data = new RuleDataModel
            {
                Conditions = new List<ConditionModel>(),
                Actions = new List<IActionModel>(),
                Enabled = true,
                Deleted = false,
                TimePeriod = 60000,  // a value from timePeriodMap, a field of the RuleReferenceDatModel
                Name = this.rand.NextString(),
                Description = this.rand.NextString(),
                GroupId = this.rand.NextString(),
                Severity = this.rand.NextString(),
                Calculation = this.rand.NextString()
            };

            return new ValueApiModel
            {
                Key = this.rand.NextString(),
                ETag = this.rand.NextString(),
                Data = JsonConvert.SerializeObject(data)
            };
        }

        [Fact]
        public async Task ConvertAsyncReturnsExpectedModel()
        {
            string tenantId = this.rand.NextString();
            List<ValueApiModel> rulesList = new List<ValueApiModel>
            {
                this.CreateRule(),
                this.CreateRule()
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