using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.TestHelpers;
using Mmm.Platform.IoT.IoTHubManager.Services;
using Mmm.Platform.IoT.IoTHubManager.WebService.Controllers;
using Mmm.Platform.IoT.IoTHubManager.WebService.Models;
using Moq;
using Xunit;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Test.Controllers
{
    public class DevicePropertiesControllerTest : IDisposable
    {
        private readonly DevicePropertiesController devicePropertiesController;
        private readonly Mock<IDeviceProperties> devicePropertiesMock;
        private bool disposedValue = false;

        public DevicePropertiesControllerTest()
        {
            this.devicePropertiesMock = new Mock<IDeviceProperties>();
            this.devicePropertiesController = new DevicePropertiesController(this.devicePropertiesMock.Object);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetPropertiesReturnExpectedResponse()
        {
            // Arrange
            this.devicePropertiesMock.Setup(x => x.GetListAsync()).ReturnsAsync(this.CreateFakeList());
            DevicePropertiesApiModel expectedModel = new DevicePropertiesApiModel(this.CreateFakeList());

            // Act
            DevicePropertiesApiModel model = await this.devicePropertiesController.GetAsync();

            // Assert
            this.devicePropertiesMock.Verify(x => x.GetListAsync(), Times.Once);
            Assert.NotNull(model);
            Assert.Equal(model.Metadata.Keys, expectedModel.Metadata.Keys);
            foreach (string key in model.Metadata.Keys)
            {
                Assert.Equal(model.Metadata[key], expectedModel.Metadata[key]);
            }
            // Assert model and expected model have same items
            Assert.Empty(model.Items.Except(expectedModel.Items));
            Assert.Empty(expectedModel.Items.Except(model.Items));
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetPropertiesThrowsException_IfDevicePropertiesThrowsException()
        {
            // Arrange
            this.devicePropertiesMock.Setup(x => x.GetListAsync()).Throws<ExternalDependencyException>();

            // Act - Assert
            await Assert.ThrowsAsync<ExternalDependencyException>(() => this.devicePropertiesController.GetAsync());

        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    devicePropertiesController.Dispose();
                }

                disposedValue = true;
            }
        }

        private List<string> CreateFakeList()
        {
            return new List<string>
            {
                "property1",
                "property2",
                "property3",
                "property4",
            };
        }
    }
}