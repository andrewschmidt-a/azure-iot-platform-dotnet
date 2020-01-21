using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.TestHelpers;
using Mmm.Platform.IoT.IdentityGateway.Services;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.IdentityGateway.WebService.Controllers;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.Test.Controllers
{
    public class UserTenantControllerTest : IDisposable
    {
        private const string SomeUserId = "someUserId";
        private const string SomeSub = "someSub";
        private const string SomeRole = "someRole";
        private const string SomeTenantId = "someTenantId";
        private bool disposedValue = false;
        private Mock<UserTenantContainer> mockUserTenantContainer;
        private UserTenantController userTenantController;
        private Mock<HttpContext> mockHttpContext;
        private UserTenantListModel someUserTenantList = new UserTenantListModel();
        private UserTenantModel someUserTenant = new UserTenantModel();
        private Mock<HttpRequest> mockHttpRequest;
        private Mock<IJwtHelpers> mockJwtHelper;
        private Mock<ISendGridClientFactory> mockSendGridClientFactory;
        private Mock<ISendGridClient> mockSendGridClient;
        private Invitation someInvitation = new Invitation { role = SomeRole };
        private JwtSecurityToken someSecurityToken;
        private Guid someTenant = Guid.NewGuid();
        private HostString someHost = new HostString("somehost");
        private IDictionary<object, object> contextItems;

        public UserTenantControllerTest()
        {
            InitializeController();
            SetupDefaultBehaviors();
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllUsersForTenantReturnsExpectedUserTenantList()
        {
            // Arrange
            // Act
            var result = await userTenantController.GetAllUsersForTenantAsync();

            // Assert
            Assert.Equal(someUserTenantList, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsGetAllTenantsForUserReturnsExpectedUserTenantList()
        {
            // Arrange
            // Act
            var result = await userTenantController.UserClaimsGetAllTenantsForUserAsync();

            // Assert
            Assert.Equal(someUserTenantList, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllTenantsForUserReturnsExpectedUserTenantList()
        {
            // Arrange
            // Act
            var result = await userTenantController.GetAllTenantsForUserAsync(SomeUserId);

            // Assert
            Assert.Equal(someUserTenantList, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsGetReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.UserClaimsGetAsync();

            // Assert
            Assert.Equal(someUserTenant, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.GetAsync(SomeUserId);

            // Assert
            Assert.Equal(someUserTenant, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PostReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.PostAsync(SomeUserId, someUserTenant);

            // Assert
            Assert.Equal(someUserTenant, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsPostReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.UserClaimsPostAsync(someUserTenant);

            // Assert
            Assert.Equal(someUserTenant, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PutReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.PutAsync(SomeUserId, someUserTenant);

            // Assert
            Assert.Equal(someUserTenant, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsPutReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.UserClaimsPutAsync(someUserTenant);

            // Assert
            Assert.Equal(someUserTenant, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task DeleteReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.DeleteAsync(SomeUserId);

            // Assert
            Assert.Equal(someUserTenant, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UserClaimsDeleteReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.UserClaimsDeleteAsync();

            // Assert
            Assert.Equal(someUserTenant, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task DeleteAllReturnsExpectedUserTenantList()
        {
            // Arrange
            // Act
            var result = await userTenantController.DeleteAllAsync();

            // Assert
            Assert.Equal(someUserTenantList, result);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task InviteReturnsExpectedUserTenant()
        {
            // Arrange
            // Act
            var result = await userTenantController.InviteAsync(someInvitation);

            // Assert
            Assert.Equal(someUserTenant, result);
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
                    userTenantController.Dispose();
                }

                disposedValue = true;
            }
        }

        private void InitializeController()
        {
            mockJwtHelper = new Mock<IJwtHelpers> { DefaultValue = DefaultValue.Mock };
            mockUserTenantContainer = new Mock<UserTenantContainer>();
            mockHttpContext = new Mock<HttpContext> { DefaultValue = DefaultValue.Mock };
            mockHttpRequest = new Mock<HttpRequest> { DefaultValue = DefaultValue.Mock };
            mockSendGridClientFactory = new Mock<ISendGridClientFactory> { DefaultValue = DefaultValue.Mock };
            mockSendGridClient = new Mock<ISendGridClient> { DefaultValue = DefaultValue.Mock };
            userTenantController = new UserTenantController(mockUserTenantContainer.Object, mockJwtHelper.Object, mockSendGridClientFactory.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };
        }

        private void SetupDefaultBehaviors()
        {
            mockUserTenantContainer.Setup(m => m.GetAllAsync(It.IsAny<UserTenantInput>())).ReturnsAsync(someUserTenantList);
            mockUserTenantContainer.Setup(m => m.GetAllUsersAsync(It.IsAny<UserTenantInput>())).ReturnsAsync(someUserTenantList);
            mockUserTenantContainer.Setup(m => m.GetAsync(It.IsAny<UserTenantInput>())).ReturnsAsync(someUserTenant);
            mockUserTenantContainer.Setup(m => m.CreateAsync(It.IsAny<UserTenantInput>())).ReturnsAsync(someUserTenant);
            mockUserTenantContainer.Setup(m => m.UpdateAsync(It.IsAny<UserTenantInput>())).ReturnsAsync(someUserTenant);
            mockUserTenantContainer.Setup(m => m.DeleteAsync(It.IsAny<UserTenantInput>())).ReturnsAsync(someUserTenant);
            mockUserTenantContainer.Setup(m => m.DeleteAllAsync(It.IsAny<UserTenantInput>())).ReturnsAsync(someUserTenantList);
            mockSendGridClientFactory.Setup(m => m.CreateSendGridClient()).Returns(mockSendGridClient.Object);
            someSecurityToken = new JwtSecurityToken(null, null, new List<Claim> { new Claim("available_tenants", someTenant.ToString()) });
            mockJwtHelper.Setup(m => m.MintToken(It.IsAny<List<Claim>>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(someSecurityToken);
            mockSendGridClient.Setup(m => m.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response(HttpStatusCode.OK, new StringContent(string.Empty), null));
            mockHttpRequest.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);
            mockHttpRequest.Setup(m => m.Host).Returns(someHost);
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);
            contextItems = new Dictionary<object, object>
            {
                {
                    RequestExtension.ContextKeyUserClaims,
                    new List<Claim> { new Claim(RequestExtension.UserObjectIdClaimType, SomeSub) }
                },
                {
                    RequestExtension.ContextKeyTenantId, SomeTenantId
                }
            };
            mockHttpContext.Setup(m => m.Items).Returns(contextItems);
        }
    }
}