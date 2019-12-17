
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.AsaManager.Services.Exceptions;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.AsaManager.Services.Models;
using Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Services.Test
{
    public class DeviceGroupsConverterTest 
    {
        private Mock<IBlobStorageClient> mockBlobStorageClient;
        private Mock<IStorageAdapterClient> mockStorageAdapterClient;
        private Mock<IIotHubManagerClient> mockIotHubManagerClient;
        private Mock<ILogger<DeviceGroupsConverter>> mockLog;
        private DeviceGroupsConverter converter;
        private readonly Random rand;

        public DeviceGroupsConverterTest () {
            this.mockBlobStorageClient = new Mock<IBlobStorageClient> ();
            this.mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            this.mockIotHubManagerClient = new Mock<IIotHubManagerClient>();
            this.mockLog = new Mock<ILogger<DeviceGroupsConverter>>();
            this.rand = new Random();

            this.converter = new DeviceGroupsConverter(
                this.mockIotHubManagerClient.Object,
                this.mockBlobStorageClient.Object,
                this.mockStorageAdapterClient.Object,
                this.mockLog.Object);
        }

        public ValueApiModel CreateDeviceGroup()
        {
            DeviceGroupDataModel data = new DeviceGroupDataModel
            {
                Conditions = new List<DeviceGroupConditionModel>(),
                DisplayName = this.rand.NextString()
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
            List<ValueApiModel> devicegroupsList = new List<ValueApiModel>
            {
                this.CreateDeviceGroup(),
                this.CreateDeviceGroup()
            };
            ValueListApiModel devicegroups = new ValueListApiModel
            {
                Items = devicegroupsList
            };

            this.mockStorageAdapterClient
                .Setup(c => c.GetAllAsync(
                    It.Is<String>(s => s == this.converter.Entity)))
                .ReturnsAsync(devicegroups);
            
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

            Assert.Equal(conversionResponse.Entities, devicegroups);
            Assert.Equal(conversionResponse.TenantId, tenantId);
        }

        [Fact]
        public async Task ConvertAsyncThrowsOnEmptyDeviceGroups()
        {
            string tenantId = this.rand.NextString();
            ValueListApiModel devicegroups = new ValueListApiModel
            {
                Items = new List<ValueApiModel>()
            };

            this.mockStorageAdapterClient
                .Setup(c => c.GetAllAsync(
                    It.Is<String>(s => s == this.converter.Entity)))
                .ReturnsAsync(devicegroups);

            Func<Task> conversion = async () => await this.converter.ConvertAsync(tenantId);

            await Assert.ThrowsAsync<EmptyEntitesException>(conversion);
        }
    }
}