using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.TestHelpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.IdentityGateway.WebService.Controllers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.Test.Controllers
{
    public class ConfigurationControllerTest : IDisposable
    {
        private const string SomePublicKey = "-----BEGIN PUBLIC KEY-----\r\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAryQICCl6NZ5gDKrnSztO\r\n3Hy8PEUcuyvg/ikC+VcIo2SFFSf18a3IMYldIugqqqZCs4/4uVW3sbdLs/6PfgdX\r\n7O9D22ZiFWHPYA2k2N744MNiCD1UE+tJyllUhSblK48bn+v1oZHCM0nYQ2NqUkvS\r\nj+hwUU3RiWl7x3D2s9wSdNt7XUtW05a/FXehsPSiJfKvHJJnGOX0BgTvkLnkAOTd\r\nOrUZ/wK69Dzu4IvrN4vs9Nes8vbwPa/ddZEzGR0cQMt0JBkhk9kU/qwqUseP1QRJ\r\n5I1jR4g8aYPL/ke9K35PxZWuDp3U0UPAZ3PjFAh+5T+fc7gzCs9dPzSHloruU+gl\r\nFQIDAQAB\r\n-----END PUBLIC KEY-----";
        private const string SomeUri = "http://azureb2caseuri.com";
        private const string SomeIssuer = "http://someissuer";
        private bool disposedValue = false;
        private ConfigurationController configurationController;
        private Mock<HttpContext> mockHttpContext;
        private Mock<AppConfig> mockAppConfig;
        private Mock<OpenIdProviderConfiguration> mockOpenIdProviderConfiguration;
        private Mock<IRsaHelpers> mockRsaHelpers;
        private JsonWebKeySet someJwks = new JsonWebKeySet();

        public ConfigurationControllerTest()
        {
            InitializeController();
            SetupDefaultBehaviors();
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetOpenIdProviderConfigurationReturnsExpectedIssuer()
        {
            // Arrange
            // Act
            var result = configurationController.GetOpenIdProviderConfiguration() as OkObjectResult;
            var openIdProviderConfiguration = result.Value as IOpenIdProviderConfiguration;

            // Assert
            Assert.Equal(SomeIssuer, openIdProviderConfiguration.Issuer);
            Assert.Equal(ConfigurationController.ContentType, result.ContentTypes[0]);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetJsonWebKeySetReturnsExpctedJwks()
        {
            // Arrange
            // Act
            var result = configurationController.GetJsonWebKeySet();
            var jwks = JsonConvert.DeserializeObject<JsonWebKeySet>(result.Content);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.Equal(someJwks.AdditionalData, jwks.AdditionalData);
            Assert.Equal(someJwks.Keys, jwks.Keys);
            Assert.Equal(ConfigurationController.ContentType, result.ContentType.ToLowerInvariant());
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
                    configurationController.Dispose();
                }

                disposedValue = true;
            }
        }

        private void InitializeController()
        {
            mockAppConfig = new Mock<AppConfig> { DefaultValue = DefaultValue.Mock };
            mockHttpContext = new Mock<HttpContext> { DefaultValue = DefaultValue.Mock };
            mockOpenIdProviderConfiguration = new Mock<OpenIdProviderConfiguration> { DefaultValue = DefaultValue.Mock };
            mockRsaHelpers = new Mock<IRsaHelpers> { DefaultValue = DefaultValue.Mock };
            configurationController = new ConfigurationController(mockAppConfig.Object, mockOpenIdProviderConfiguration.Object, mockRsaHelpers.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = this.mockHttpContext.Object
                }
            };
        }

        private void SetupDefaultBehaviors()
        {
            mockAppConfig.Setup(m => m.Global.AzureB2cBaseUri).Returns(SomeUri);
            mockAppConfig.Setup(m => m.IdentityGatewayService.PublicKey).Returns(SomePublicKey);
            mockOpenIdProviderConfiguration.SetupGet(m => m.Issuer).Returns(SomeIssuer);
            mockRsaHelpers.Setup(m => m.GetJsonWebKey(It.IsAny<string>())).Returns(someJwks);
        }
    }
}
