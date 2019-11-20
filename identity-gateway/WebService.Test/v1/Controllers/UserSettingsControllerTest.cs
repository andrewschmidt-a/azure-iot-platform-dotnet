using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.IdentityGateway.WebService.v1.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.AuthUtils;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Xunit;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.Test.v1.Controllers
{
    public class UserSettingsControllerTest
    {
        private Mock<UserSettingsContainer> mockUserSettingsContainer;
        private UserSettingsController userSettingsController;
        private Mock<HttpContext> mockHttpContext;
        private UserSettingsListModel someUserSettingsList = new UserSettingsListModel();
        private UserSettingsModel someUserSettings = new UserSettingsModel();
        private Mock<HttpRequest> mockHttpRequest;
        private const string someUserId = "someUserId";
        private const string someSub = "someSub";
        private const string someSetting = "someSetting";
        private const string someValue = "someValue";

        public UserSettingsControllerTest()
        {
            InitializeController();
            SetupDefaultBehaviors();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllReturnsExpectedUserSettingsList()
        {
            // Arrange
            // Act
            var result = await userSettingsController.GetAllAsync(someUserId);

            // Assert
            Assert.Equal(someUserSettingsList, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsGetAllReturnsExpectedUserSettingsList()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsGetAllAsync();

            // Assert
            Assert.Equal(someUserSettingsList, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.GetAsync(someUserId, someSetting);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsGetReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsGetAsync(someSetting);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PostReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.PostAsync(someUserId, someSetting, someValue);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsPostReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsPostAsync(someSetting, someValue);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PutReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.PutAsync(someUserId, someSetting, someValue);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsPutReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsPutAsync(someSetting, someValue);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task DeleteReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.DeleteAsync(someUserId, someSetting);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsDeleteReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsController.UserClaimsDeleteAsync(someSetting);

            // Assert
            Assert.Equal(someUserSettings, result);
        }

        private void InitializeController()
        {
            mockUserSettingsContainer = new Mock<UserSettingsContainer>();
            mockHttpContext = new Mock<HttpContext> { DefaultValue = DefaultValue.Mock };
            mockHttpContext.SetupAllProperties();
            mockHttpRequest = new Mock<HttpRequest> { DefaultValue = DefaultValue.Mock };
            mockHttpRequest.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);
            userSettingsController = new UserSettingsController(mockUserSettingsContainer.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };
        }

        private void SetupDefaultBehaviors()
        {
            mockUserSettingsContainer.Setup(m => m.GetAllAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettingsList);
            mockUserSettingsContainer.Setup(m => m.GetAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettings);
            mockUserSettingsContainer.Setup(m => m.CreateAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettings);
            mockUserSettingsContainer.Setup(m => m.UpdateAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettings);
            mockUserSettingsContainer.Setup(m => m.DeleteAsync(It.IsAny<UserSettingsInput>())).ReturnsAsync(someUserSettings);
            mockHttpContext.Object.Items = new Dictionary<object, object> { { RequestExtension.ContextKeyUserClaims, new List<Claim> { new Claim(RequestExtension.UserObjectIdClaimType, someSub) } } };
        }
    }
}
