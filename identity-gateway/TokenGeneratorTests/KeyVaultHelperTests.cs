using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using TokenGenerator.Helpers;

namespace TokenGeneratorTests
{
    [TestClass]
    public class KeyVaultHelperTests
    {
        private IConfiguration _config;
        private string keyVaultName = new Guid().ToString();
        private string keyVaultAppId = new Guid().ToString();
        private string keyVaultAppKey = new Guid().ToString();
        private string tenantId = new Guid().ToString();
        private string secret = new Guid().ToString();

        public KeyVaultHelperTests()
        {
            // Arrange
            var mock = new Mock<IConfiguration>();

            mock.Setup(foo => foo["keyvaultName"]).Returns(keyVaultName);
            mock.Setup(foo => foo["keyvaultAppId"]).Returns(keyVaultAppId);
            mock.Setup(foo => foo["keyvaultAppKey"]).Returns(keyVaultAppKey);
            mock.Setup(foo => foo["tenantId"]).Returns(tenantId);
            this._config = mock.Object;
        }
        [TestMethod]
        public void TestSecretIdentifier()
        {
            // Act
            var secretID = (new KeyVaultHelper(this._config)).getKeyVaultSecretIdentifier(secret);

            // Assert
            Assert.AreEqual(secretID, $"https://{keyVaultName}.vault.azure.net/secrets/{secret}");

        }
    }
}
