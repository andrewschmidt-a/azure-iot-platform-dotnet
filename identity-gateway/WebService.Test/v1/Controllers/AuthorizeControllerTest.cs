using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using IdentityGateway.Controllers;
using IdentityGateway.Services;
using IdentityGateway.Services.Exceptions;
using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using IdentityGateway.Services.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.v1.Controllers
{
    public class AuthorizeControllerTest
    {
        private Mock<IUserContainer<UserSettingsModel, UserSettingsInput>> mockUserSettingsContainer;
        private Mock<UserTenantContainer> mockUserTenantContainer;
        private AuthorizeController authorizeController;
        private string someUiRedirectUri = new Uri("http://valid-uri.com").AbsoluteUri;
        private Guid someTenant = Guid.NewGuid();
        private Mock<HttpContext> mockHttpContext;
        private string state = "someState";
        private string clientId = "someClientId";
        private string nonce = "someNonce";
        private const string someIssuer = "http://someIssuer";
        private string invite = "someInvite";
        private const string somePublicKey = "-----BEGIN PUBLIC KEY-----\r\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAryQICCl6NZ5gDKrnSztO\r\n3Hy8PEUcuyvg/ikC+VcIo2SFFSf18a3IMYldIugqqqZCs4/4uVW3sbdLs/6PfgdX\r\n7O9D22ZiFWHPYA2k2N744MNiCD1UE+tJyllUhSblK48bn+v1oZHCM0nYQ2NqUkvS\r\nj+hwUU3RiWl7x3D2s9wSdNt7XUtW05a/FXehsPSiJfKvHJJnGOX0BgTvkLnkAOTd\r\nOrUZ/wK69Dzu4IvrN4vs9Nes8vbwPa/ddZEzGR0cQMt0JBkhk9kU/qwqUseP1QRJ\r\n5I1jR4g8aYPL/ke9K35PxZWuDp3U0UPAZ3PjFAh+5T+fc7gzCs9dPzSHloruU+gl\r\nFQIDAQAB\r\n-----END PUBLIC KEY-----";
        private const string someUri = "http://azureb2caseuri.com";
        private Mock<IServicesConfig> mockServicesConfig;
        private Mock<IJwtHelpers> mockJwtHelper;
        private JwtSecurityToken someSecurityToken;
        public static readonly string ValidAuthHeader = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        private Mock<IOpenIdProviderConfiguration> mockOpenIdProviderConfiguration;

        public AuthorizeControllerTest()
        {
            InitializeController();
            SetupDefaultBehaviors();
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("not-a-valid-uri")]
        [InlineData(null)]
        [InlineData("")]
        public void AuthorizeThrowsWhenRedirectUriNotValidTest(string invalidUri)
        {
            // Arrange
            // Act
            Action a = () => authorizeController.Get(invalidUri, null, null, null, null, null);

            // Assert
            Assert.Throws<Exception>(a);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("x")]
        [InlineData("7")]
        [InlineData("")]
        [InlineData("not-a-valid-guid")]
        public void AuthorizeThrowsWhenTenantNotValidTest(string invalidTenant)
        {
            // Arrange
            // Act
            Action a = () => authorizeController.Get(someUiRedirectUri, null, null, null, invalidTenant, null);

            // Assert
            Assert.Throws<Exception>(a);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void AuthorizeRedirectsToRedirectUriTest()
        {
            // Arrange
            // Act
            var redirectResult = authorizeController.Get(someUiRedirectUri, state, clientId, nonce, someTenant.ToString(), invite) as RedirectResult;

            // Assert
            Assert.NotNull(redirectResult);
            var uriResult = new Uri(redirectResult.Url);
            Assert.NotNull(uriResult.Query);
            Assert.NotEmpty(uriResult.Query);
            var queryStrings = HttpUtility.ParseQueryString(uriResult.Query);
            Assert.Contains("state", queryStrings.AllKeys);
            var returnedState = JObject.Parse(queryStrings["state"]);
            Assert.Equal(someUiRedirectUri, returnedState["returnUrl"]);
            Assert.Equal(state, returnedState["state"]);
            Assert.Equal(someTenant, returnedState["tenant"]);
            Assert.Equal(nonce, returnedState["nonce"]);
            Assert.Equal(clientId, returnedState["client_id"]);
            Assert.Equal($"{someIssuer}/connect/callback", queryStrings["redirect_uri"]);
            Assert.Equal(invite, returnedState["invitation"]);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("not-a-valid-uri")]
        [InlineData(null)]
        [InlineData("")]
        public void LogoutThrowsWhenRedirectUriNotValidTest(string invalidUri)
        {
            // Arrange
            // Act
            Action a = () => authorizeController.Get(invalidUri);

            // Assert
            Assert.Throws<Exception>(a);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void LogoutRedirectsToRedirectUriTest()
        {
            // Arrange
            // Act
            var redirectResult = authorizeController.Get(someUiRedirectUri) as RedirectResult;

            // Assert
            Assert.Equal(someUiRedirectUri, redirectResult.Url);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("not-a-valid-auth-header")]
        [InlineData(null)]
        [InlineData("")]
        public async Task SwitchTenantThrowsWhenAuthorizationHeaderNotValidTest(string invalidAuthHeader)
        {
            // Arrange
            // Act
            Func<Task> a = async () => await authorizeController.PostAsync(invalidAuthHeader, null);

            // Assert
            await Assert.ThrowsAsync<NoAuthorizationException>(a);
        }

        public static IEnumerable<object[]> GetInvalidAuthHeaders()
        {
            yield return new object[] { "Bearer not-a-valid-auth-header" };
            yield return new object[] { "Bearer " };
            yield return new object[] { ValidAuthHeader };
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [MemberData(nameof(GetInvalidAuthHeaders))]
        public async Task SwitchTenantThrowsWhenAuthorizationHeaderTokenNotReadableOrValidTest(string invalidAuthHeader)
        {
            // Arrange
            JwtSecurityToken jwtSecurityToken = null;
            mockJwtHelper.Setup(m => m.TryValidateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpContext>(), out jwtSecurityToken)).Returns(false);

            // Act
            Func<Task> a = async () => await authorizeController.PostAsync(invalidAuthHeader, null);

            // Assert
            await Assert.ThrowsAsync<NoAuthorizationException>(a);
        }

        public static IEnumerable<object[]> GetJwtSecurityTokens()
        {
            yield return new object[] { null };
            yield return new object[] { new JwtSecurityToken(null, null, new List<Claim> { new Claim("available_tenants", Guid.NewGuid().ToString()) }) };
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [MemberData(nameof(GetJwtSecurityTokens))]
        public async Task SwitchTenantThrowsWhenTenantAccessNotAllowed(JwtSecurityToken jwtSecurityToken)
        {
            // Arrange
            mockJwtHelper.Setup(m => m.TryValidateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpContext>(), out jwtSecurityToken)).Returns(true);

            // Act
            Func<Task> a = async () => await authorizeController.PostAsync(ValidAuthHeader, someTenant.ToString());

            // Assert
            await Assert.ThrowsAsync<NoAuthorizationException>(a);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task SwitchTenantMintsNewTokenWithNewTenant()
        {
            // Arrange
            var currentTenant = someTenant;
            var availableTenant = Guid.NewGuid();
            var claimWithCurrentTenant = new Claim("available_tenants", currentTenant.ToString());
            var claimWithAvailableTenant = new Claim("available_tenants", availableTenant.ToString());
            var subClaim = new Claim("sub", "someSub");
            var audClaim = new Claim("aud", "someAud");
            var expClaim = new Claim("exp", "1571240635");
            var jwtSecurityToken = new JwtSecurityToken(null, null, new List<Claim> { claimWithCurrentTenant, claimWithAvailableTenant, subClaim, audClaim, expClaim });

            // return sucessfully a UserTenant
            mockUserTenantContainer.Setup(s => s.GetAsync(It.IsAny<UserTenantInput>())).ReturnsAsync(new UserTenantModel("test", "test"));

            mockJwtHelper.Setup(m => m.TryValidateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpContext>(), out jwtSecurityToken)).Returns(true);
            mockJwtHelper.Setup(m => m.GetIdentityToken(It.IsAny<List<Claim>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>())).ReturnsAsync(jwtSecurityToken);

            // Act
            var objectResult = await authorizeController.PostAsync(ValidAuthHeader, availableTenant.ToString()) as ObjectResult;
            
            // Assert
            Assert.Equal(objectResult.StatusCode, StatusCodes.Status200OK);
        }

        private void InitializeController()
        {
            mockServicesConfig = new Mock<IServicesConfig> { DefaultValue = DefaultValue.Mock };
            mockUserTenantContainer = new Mock<UserTenantContainer>();

            mockUserSettingsContainer = new Mock<IUserContainer<UserSettingsModel, UserSettingsInput>>();
            mockJwtHelper = new Mock<IJwtHelpers> { DefaultValue = DefaultValue.Mock };
            mockHttpContext = new Mock<HttpContext> { DefaultValue = DefaultValue.Mock };
            mockOpenIdProviderConfiguration = new Mock<IOpenIdProviderConfiguration> {DefaultValue = DefaultValue.Mock};
            authorizeController = new AuthorizeController(mockServicesConfig.Object, mockUserTenantContainer.Object, mockUserSettingsContainer.Object as UserSettingsContainer, mockJwtHelper.Object, mockOpenIdProviderConfiguration.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = this.mockHttpContext.Object
                }
            };
        }

        private void SetupDefaultBehaviors()
        {
            mockServicesConfig.Setup(m => m.AzureB2CBaseUri).Returns(someUri);
            mockServicesConfig.Setup(m => m.PublicKey).Returns(somePublicKey);
            someSecurityToken = new JwtSecurityToken(null, null, new List<Claim> { new Claim("available_tenants", someTenant.ToString()) });
            mockJwtHelper.Setup(m => m.TryValidateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpContext>(), out someSecurityToken)).Returns(true);
            mockOpenIdProviderConfiguration.Setup(m => m.issuer).Returns(someIssuer);
        }
    }
}
