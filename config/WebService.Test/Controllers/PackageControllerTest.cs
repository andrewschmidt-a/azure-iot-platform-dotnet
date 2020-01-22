// <copyright file="PackageControllerTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Mmm.Iot.Common.TestHelpers;
using Mmm.Iot.Config.Services;
using Mmm.Iot.Config.Services.Models;
using Mmm.Iot.Config.WebService.Controllers;
using Moq;
using Xunit;

namespace Mmm.Iot.Config.WebService.Test.Controllers
{
    public class PackageControllerTest : IDisposable
    {
        private const string DateFormat = "yyyy-MM-dd'T'HH:mm:sszzz";
        private readonly Mock<IStorage> mockStorage;
        private readonly PackagesController controller;
        private readonly Random rand;
        private bool disposedValue = false;

        public PackageControllerTest()
        {
            this.mockStorage = new Mock<IStorage>();
            this.controller = new PackagesController(this.mockStorage.Object);
            this.rand = new Random();
        }

        [Theory]
        [Trait(Constants.Type, Constants.UnitTest)]
        [InlineData("EdgeManifest", "filename", true, false)]
        [InlineData("EdgeManifest", "filename", false, true)]
        [InlineData("EdgeManifest", "filename", false, true, true)]
        [InlineData("DeviceConfiguration", "filename", true, false, true)]
        [InlineData("EdgeManifest", "", true, true)]
        [InlineData("BAD_TYPE", "filename", true, true)]
        public async Task PostAsyncExceptionVerificationTest(
            string type,
            string filename,
            bool isValidFileProvided,
            bool expectException,
            bool shouldHaveConfig = false)
        {
            // Arrange
            IFormFile file = null;
            if (isValidFileProvided)
            {
                bool isEdgePackage = (type == "EdgeManifest") ? true : false;
                file = this.CreateSampleFile(filename, isEdgePackage);
            }

            Enum.TryParse(type, out PackageType pckgType);

            this.mockStorage.Setup(x => x.AddPackageAsync(
                                    It.Is<PackageServiceModel>(p => p.PackageType.ToString().Equals(type) &&
                                                        p.Name.Equals(filename))))
                            .ReturnsAsync(new PackageServiceModel()
                            {
                                Name = filename,
                                PackageType = pckgType,
                            });

            var configType = shouldHaveConfig ? "customconfig" : null;

            try
            {
                // Act
                var package = await this.controller.PostAsync(type, configType, file);

                // Assert
                Assert.False(expectException);
                Assert.Equal(filename, package.Name);
                Assert.Equal(type, package.PackageType.ToString());
            }
            catch (Exception)
            {
                Assert.True(expectException);
            }
        }

        [Fact]
        public async Task GetPackageTest()
        {
            // Arrange
            const string id = "packageId";
            const string name = "packageName";
            const PackageType type = PackageType.EdgeManifest;
            const string content = "{}";
            string dateCreated = DateTime.UtcNow.ToString(DateFormat);

            this.mockStorage
                .Setup(x => x.GetPackageAsync(id))
                .ReturnsAsync(new PackageServiceModel()
                {
                    Id = id,
                    Name = name,
                    Content = content,
                    PackageType = type,
                    ConfigType = string.Empty,
                    DateCreated = dateCreated,
                });

            // Act
            var pkg = await this.controller.GetAsync(id);

            // Assert
            this.mockStorage
                .Verify(x => x.GetPackageAsync(id), Times.Once);

            Assert.Equal(id, pkg.Id);
            Assert.Equal(name, pkg.Name);
            Assert.Equal(type, pkg.PackageType);
            Assert.Equal(content, pkg.Content);
            Assert.Equal(dateCreated, pkg.DateCreated);
        }

        [Fact]
        public async Task GetAllPackageTest()
        {
            // Arrange
            const string id = "packageId";
            const string name = "packageName";
            const PackageType type = PackageType.EdgeManifest;
            string config = string.Empty;
            const string content = "{}";
            string dateCreated = DateTime.UtcNow.ToString(DateFormat);

            int[] idx = new int[] { 0, 1, 2 };
            var packages = idx.Select(i => new PackageServiceModel()
            {
                Id = id + i,
                Name = name + i,
                Content = content + i,
                PackageType = type,
                ConfigType = config + i,
                DateCreated = dateCreated,
            }).ToList();

            this.mockStorage
                .Setup(x => x.GetAllPackagesAsync())
                .ReturnsAsync(packages);

            // Act
            var resultPackages = await this.controller.GetFilteredAsync(null, null);

            // Assert
            this.mockStorage
                .Verify(x => x.GetAllPackagesAsync(), Times.Once);

            foreach (int i in idx)
            {
                var pkg = resultPackages.Items.ElementAt(i);
                Assert.Equal(id + i, pkg.Id);
                Assert.Equal(name + i, pkg.Name);
                Assert.Equal(type, pkg.PackageType);
                Assert.Equal(content + i, pkg.Content);
                Assert.Equal(dateCreated, pkg.DateCreated);
            }
        }

        [Fact]
        public async Task ListPackagesTest()
        {
            // Arrange
            const string id = "packageId";
            const string name = "packageName";
            const PackageType type = PackageType.DeviceConfiguration;
            const string content = "{}";
            string dateCreated = DateTime.UtcNow.ToString(DateFormat);

            int[] idx = new int[] { 0, 1, 2 };
            var packages = idx.Select(i => new PackageServiceModel()
            {
                Id = id + i,
                Name = name + i,
                Content = content + i,
                PackageType = type + i,
                ConfigType = (i == 0) ? ConfigType.Firmware.ToString() : i.ToString(),
                DateCreated = dateCreated,
            }).ToList();

            this.mockStorage
                .Setup(x => x.GetFilteredPackagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(packages);

            // Act
            var resultPackages = await this.controller.GetFilteredAsync(
                                                    PackageType.DeviceConfiguration.ToString(),
                                                    ConfigType.Firmware.ToString());

            // Assert
            this.mockStorage.Verify(
                x => x.GetFilteredPackagesAsync(
                    PackageType.DeviceConfiguration.ToString(),
                    ConfigType.Firmware.ToString()),
                Times.Once);
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
                    controller.Dispose();
                }

                disposedValue = true;
            }
        }

        private FormFile CreateSampleFile(string filename, bool isEdgePackage)
        {
            var admPackage = "{\"id\":\"dummy\",\"content\":{\"deviceContent\":{}}}";
            var edgePackage = "{\"id\":\"dummy\",\"content\":{\"modulesContent\":{}}}";

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var package = isEdgePackage ? edgePackage : admPackage;

            writer.Write(package);
            writer.Flush();
            stream.Position = 0;

            return new FormFile(stream, 0, package.Length, "file", filename);
        }
    }
}