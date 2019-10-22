using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using IdentityGateway.Services.Runtime;
using IdentityGateway.WebService.v1.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.v1.Controllers
{
    public class ConfigurationControllerTest
    {
        private ConfigurationController controller;
        private Mock<HttpContext> mockHttpContext;
        private const string somePublicKey = "-----BEGIN PUBLIC KEY-----\r\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAryQICCl6NZ5gDKrnSztO\r\n3Hy8PEUcuyvg/ikC+VcIo2SFFSf18a3IMYldIugqqqZCs4/4uVW3sbdLs/6PfgdX\r\n7O9D22ZiFWHPYA2k2N744MNiCD1UE+tJyllUhSblK48bn+v1oZHCM0nYQ2NqUkvS\r\nj+hwUU3RiWl7x3D2s9wSdNt7XUtW05a/FXehsPSiJfKvHJJnGOX0BgTvkLnkAOTd\r\nOrUZ/wK69Dzu4IvrN4vs9Nes8vbwPa/ddZEzGR0cQMt0JBkhk9kU/qwqUseP1QRJ\r\n5I1jR4g8aYPL/ke9K35PxZWuDp3U0UPAZ3PjFAh+5T+fc7gzCs9dPzSHloruU+gl\r\nFQIDAQAB\r\n-----END PUBLIC KEY-----";
        private const string someUri = "http://azureb2caseuri.com";
        private Mock<IServicesConfig> mockServicesConfig;
        private Mock<OpenIdProviderConfiguration> mockOpenIdProviderConfiguration;
        private Mock<IRsaHelpers> mockRsaHelpers;
        private const string someIssuer = "http://someissuer";
        private JsonWebKeySet someJwks = new JsonWebKeySet();

        public ConfigurationControllerTest()
        {
            InitializeController();
            SetupDefaultBehaviors();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetOpenIdProviderConfigurationReturnsExpectedIssuer()
        {
            // Arrange
            //Act
            var result = controller.GetOpenIdProviderConfiguration() as OkObjectResult;
            var openIdProviderConfiguration = result.Value as IOpenIdProviderConfiguration;

            // Assert
            Assert.Equal(someIssuer, openIdProviderConfiguration.issuer);
            Assert.Equal(ConfigurationController.ContentType, result.ContentTypes[0]);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetJsonWebKeySetReturnsExpctedJwks()
        {
            // Arrange
            // Act
            var result = controller.GetJsonWebKeySet();
            var jwks = JsonConvert.DeserializeObject<JsonWebKeySet>(result.Content);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.Equal(someJwks.AdditionalData, jwks.AdditionalData);
            Assert.Equal(someJwks.Keys, jwks.Keys);
            Assert.Equal(ConfigurationController.ContentType, result.ContentType.ToLowerInvariant());
        }

        private void InitializeController()
        {
            mockServicesConfig = new Mock<IServicesConfig> { DefaultValue = DefaultValue.Mock };
            mockHttpContext = new Mock<HttpContext> { DefaultValue = DefaultValue.Mock };
            mockOpenIdProviderConfiguration = new Mock<OpenIdProviderConfiguration> { DefaultValue = DefaultValue.Mock };
            mockRsaHelpers = new Mock<IRsaHelpers> {DefaultValue = DefaultValue.Mock};
            controller = new ConfigurationController(mockServicesConfig.Object, mockOpenIdProviderConfiguration.Object, mockRsaHelpers.Object)
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
            mockOpenIdProviderConfiguration.SetupGet(m => m.issuer).Returns(someIssuer);
            mockRsaHelpers.Setup(m => m.GetJsonWebKey(It.IsAny<string>())).Returns(someJwks);
        }
    }
}
