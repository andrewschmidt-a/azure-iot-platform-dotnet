// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Config.Services;
using Mmm.Platform.IoT.Config.Services.External;
using Mmm.Platform.IoT.Config.Services.Models;
using Mmm.Platform.IoT.Config.Services.Models.Actions;
using Mmm.Platform.IoT.Config.WebService.v1.Controllers;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Xunit;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.Config.WebService.Test.Controllers
{
    public class SolutionControllerTest : IDisposable
    {
        private bool disposedValue = false;
        private readonly Mock<IStorage> mockStorage;
        private readonly Mock<IActions> mockActions;
        private readonly Mock<ILogger<SolutionSettingsController>> _logger;
        private readonly Mock<IAzureResourceManagerClient> mockResourceManagementClient;
        private readonly SolutionSettingsController controller;
        private readonly Random rand;

        public SolutionControllerTest()
        {
            this.mockStorage = new Mock<IStorage>();
            this.mockActions = new Mock<IActions>();
            _logger = new Mock<ILogger<SolutionSettingsController>>();
            this.mockResourceManagementClient = new Mock<IAzureResourceManagerClient>();
            this.controller = new SolutionSettingsController(
                this.mockStorage.Object,
                this.mockActions.Object);
            this.rand = new Random();
        }

        [Fact]
        public async Task GetThemeAsyncTest()
        {
            var name = this.rand.NextString();
            var description = this.rand.NextString();

            this.mockStorage
                .Setup(x => x.GetThemeAsync())
                .ReturnsAsync(new
                {
                    Name = name,
                    Description = description
                });

            var result = await this.controller.GetThemeAsync() as dynamic;

            this.mockStorage
                .Verify(x => x.GetThemeAsync(), Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task SetThemeAsyncTest()
        {
            var name = this.rand.NextString();
            var description = this.rand.NextString();

            this.mockStorage
                .Setup(x => x.SetThemeAsync(It.IsAny<object>()))
                .ReturnsAsync(new
                {
                    Name = name,
                    Description = description
                });

            var result = await this.controller.SetThemeAsync(new
            {
                Name = name,
                Description = description
            }) as dynamic;

            this.mockStorage
                .Verify(
                    x => x.SetThemeAsync(
                        It.Is<object>(o => this.CheckTheme(o, name, description))),
                    Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task GetLogoShouldReturnDefaultLogo()
        {
            using (var mockContext = new MockHttpContext())
            {
                this.controller.ControllerContext.HttpContext = mockContext.Object;

                this.mockStorage
                    .Setup(x => x.GetLogoAsync())
                    .ReturnsAsync(new Logo
                    {
                        Image = Logo.Default.Image,
                        Type = Logo.Default.Type,
                        IsDefault = true
                    });

                await this.controller.GetLogoAsync();

                this.mockStorage
                    .Verify(x => x.GetLogoAsync(), Times.Once);

                Assert.Equal(Logo.Default.Image, mockContext.GetBody());
                Assert.Equal(Logo.Default.Type, mockContext.Object.Response.ContentType);
                Assert.Equal("True", mockContext.GetHeader(Logo.IS_DEFAULT_HEADER));
            }
        }

        [Fact]
        public async Task GetLogoShouldReturnExpectedNameAndType()
        {
            var image = this.rand.NextString();
            var type = this.rand.NextString();
            var name = this.rand.NextString();

            using (var mockContext = new MockHttpContext())
            {
                this.controller.ControllerContext.HttpContext = mockContext.Object;

                this.mockStorage
                    .Setup(x => x.GetLogoAsync())
                    .ReturnsAsync(new Logo
                    {
                        Image = image,
                        Type = type,
                        Name = name,
                        IsDefault = false
                    });

                await this.controller.GetLogoAsync();

                this.mockStorage
                    .Verify(x => x.GetLogoAsync(), Times.Once);

                Assert.Equal(image, mockContext.GetBody());
                Assert.Equal(type, mockContext.Object.Response.ContentType);
                Assert.Equal(name, mockContext.GetHeader(Logo.NAME_HEADER));
                Assert.Equal("False", mockContext.GetHeader(Logo.IS_DEFAULT_HEADER));
            }
        }

        [Fact]
        public async Task SetLogoShouldReturnGivenLogo()
        {
            var image = this.rand.NextString();
            var type = this.rand.NextString();

            using (var mockContext = new MockHttpContext())
            {
                this.controller.ControllerContext.HttpContext = mockContext.Object;

                this.mockStorage
                    .Setup(x => x.SetLogoAsync(It.IsAny<Logo>()))
                    .ReturnsAsync((Logo logo) => logo);

                mockContext.Object.Request.ContentType = type;
                mockContext.SetBody(image);

                await this.controller.SetLogoAsync();

                this.mockStorage
                    .Verify(
                        x => x.SetLogoAsync(
                            It.Is<Logo>(m => m.Image == image && m.Type == type && !m.IsDefault)),
                        Times.Once);

                Assert.Equal(image, mockContext.GetBody());
                Assert.Equal(type, mockContext.Object.Response.ContentType);
                Assert.Equal("False", mockContext.GetHeader(Logo.IS_DEFAULT_HEADER));
            }
        }

        [Fact]
        public async Task SetLogoShouldReturnGivenLogoAndName()
        {
            var image = this.rand.NextString();
            var type = this.rand.NextString();
            var name = this.rand.NextString();

            using (var mockContext = new MockHttpContext())
            {
                this.controller.ControllerContext.HttpContext = mockContext.Object;

                this.mockStorage
                    .Setup(x => x.SetLogoAsync(It.IsAny<Logo>()))
                    .ReturnsAsync((Logo logo) => logo);

                mockContext.Object.Request.ContentType = type;
                mockContext.SetBody(image);
                mockContext.SetHeader(Logo.NAME_HEADER, name);

                await this.controller.SetLogoAsync();

                this.mockStorage
                    .Verify(
                        x => x.SetLogoAsync(
                            It.Is<Logo>(m => m.Image == image && m.Type == type && m.Name == name && !m.IsDefault)),
                        Times.Once);

                Assert.Equal(image, mockContext.GetBody());
                Assert.Equal(type, mockContext.Object.Response.ContentType);
                Assert.Equal(name, mockContext.GetHeader(Logo.NAME_HEADER));
                Assert.Equal("False", mockContext.GetHeader(Logo.IS_DEFAULT_HEADER));
            }
        }

        [Fact]
        public async Task GetActionsReturnsListOfActions()
        {
            // Arrange
            using (var mockContext = new MockHttpContext())
            {
                this.controller.ControllerContext.HttpContext = mockContext.Object;

                var config = new AppConfig();
                var action = new EmailActionSettings(this.mockResourceManagementClient.Object, config, new Mock<ILogger<EmailActionSettings>>().Object);
                var actionsList = new List<IActionSettings>
                {
                    action
                };
                this.mockActions
                    .Setup(x => x.GetListAsync())
                    .ReturnsAsync(actionsList);

                // Act
                var result = await this.controller.GetActionsSettingsAsync();

                // Assert
                this.mockActions.Verify(x => x.GetListAsync(), Times.Once);

                Assert.NotEmpty(result.Items);
                Assert.Equal(actionsList.Count, result.Items.Count);
            }
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

        private bool CheckTheme(object obj, string name, string description)
        {
            var dynamiceObj = obj as dynamic;
            return dynamiceObj.Name.ToString() == name && dynamiceObj.Description.ToString() == description;
        }
    }
}
