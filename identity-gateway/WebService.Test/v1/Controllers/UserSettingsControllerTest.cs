// <copyright file="UserSettingsControllerTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.TestHelpers;
using Mmm.Platform.IoT.IdentityGateway.Services;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.IdentityGateway.WebService.Controllers;
using Moq;
using Xunit;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.Test.Controllers
{
    public class UserSettingsControllerTest : IDisposable
    {
        private const string SomeUserId = "someUserId";
        private const string SomeSub = "someSub";
        private const string SomeSetting = "someSetting";
        private const string SomeValue = "someValue";
        private Mock<HttpRequest> mockHttpRequest;
        private bool disposedValue = false;
        private Mock<UserSettingsContainer> mockUserSettingsContainer;
        private UserSettingsController userSettingsController;
        private Mock<HttpContext> mockHttpContext;
        private UserSettingsListModel someUserSettingsList = new UserSettingsListModel();
        private UserSettingsModel someUserSettings = new UserSettingsModel();
        private IDictionary<object, object> contextItems;

        public UserSettingsControllerTest()
        {
            InitializeController();
            SetupDefaultBehaviors();
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetAllReturnsExpectedUserSettingsList()
        {
            // Arrange
            // Act
            var result = await userSettingsController.GetAllAsync(SomeUserId);

            // Assert
            Assert.Equal(someUserSettingsList, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task UserClaimsGetAllReturnsExpectedUserSettingsList()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsGetAllAsync();

            // Assert
            Assert.Equal(someUserSettingsList, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task GetReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.GetAsync(SomeUserId, SomeSetting);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task UserClaimsGetReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsGetAsync(SomeSetting);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task PostReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.PostAsync(SomeUserId, SomeSetting, SomeValue);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task UserClaimsPostReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsPostAsync(SomeSetting, SomeValue);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task PutReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.PutAsync(SomeUserId, SomeSetting, SomeValue);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task UserClaimsPutReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsPutAsync(SomeSetting, SomeValue);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task DeleteReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.DeleteAsync(SomeUserId, SomeSetting);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task UserClaimsDeleteReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsDeleteAsync(SomeSetting);

            // Assert
            Assert.Equal(someUserSettings, result);
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
                    userSettingsController.Dispose();
                }

                disposedValue = true;
            }
        }

        private void InitializeController()
        {
            mockUserSettingsContainer = new Mock<UserSettingsContainer>();
            mockHttpContext = new Mock<HttpContext> { DefaultValue = DefaultValue.Mock };
            mockHttpRequest = new Mock<HttpRequest> { DefaultValue = DefaultValue.Mock };
            userSettingsController = new UserSettingsController(mockUserSettingsContainer.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object,
                },
            };
        }

        private void SetupDefaultBehaviors()
        {
            mockUserSettingsContainer.Setup(m => m.GetAllAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettingsList);
            mockUserSettingsContainer.Setup(m => m.GetAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettings);
            mockUserSettingsContainer.Setup(m => m.CreateAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettings);
            mockUserSettingsContainer.Setup(m => m.UpdateAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettings);
            mockUserSettingsContainer.Setup(m => m.DeleteAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettings);
            mockHttpRequest.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);
            contextItems = new Dictionary<object, object>
            {
                {
                    RequestExtension.ContextKeyUserClaims,
                    new List<Claim> { new Claim(RequestExtension.UserObjectIdClaimType, SomeSub) }
                },
            };
            mockHttpContext.Setup(m => m.Items).Returns(contextItems);
        }
    }
}