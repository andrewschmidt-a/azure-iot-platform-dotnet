using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using tenant_manager.Helpers;
using tenant_manager.Models;
using TokenGenerator.Controllers;
using TokenGenerator.Helpers;

namespace TokenGeneratorTests
{
    [TestClass]
    public class AuthorizeControllerTest
    {
        private IConfiguration _config;
        private string keyVaultName = new Guid().ToString();
        private string keyVaultAppId = new Guid().ToString();
        private string keyVaultAppKey = new Guid().ToString();
        private string tenantId = new Guid().ToString();
        private string secret = new Guid().ToString();
        private string iotTenantId = new Guid().ToString();
        private string userId = new Guid().ToString();

        public AuthorizeController ac;


        public AuthorizeControllerTest()
        {

            var mock = new Mock<IConfiguration>();

            mock.Setup(foo => foo["keyvaultName"]).Returns(keyVaultName);
            mock.Setup(foo => foo["keyvaultAppId"]).Returns(keyVaultAppId);
            mock.Setup(foo => foo["keyvaultAppKey"]).Returns(keyVaultAppKey);
            mock.Setup(foo => foo["tenantId"]).Returns(tenantId);
            this._config = mock.Object;
            // Arrange
            var keyVaultHelper = new Mock<KeyVaultHelper>();
            keyVaultHelper.Setup(t => t.getSecretAsync("tenantStorageAccountConnectionString")).Returns(Task.FromResult("conn_string"));
            keyVaultHelper.Setup(t => t.getSecretAsync("identityGatewayPrivateKey")).Returns(Task.FromResult("-----BEGIN RSA PRIVATE KEY-----MIIEpAIBAAKCAQEAtTeAt2dQsoPZLfZTJyyOL02+vfbs2vHsYhzTPnk2Sqo0l9OemoDXHhR+obEmx+lwmP/1FKcUHQ6H0pyeAz/lyv7sWdtfZv6tnzBeskv6K/0OxZxhYwnq6K39XneFPjlwkKJMozl0jkhdbvpyqwXFxN85VVRQnszjwkiC8JlGZYtP6mulh6jRFk7UJMyQp0VxRNwOXpzL2BLAlFHxGmWlaM6ITCcUOFbpwU03p1sW6yY591hBlBEzTn1OYGqBl1mCkBaMzflvOnPrYkPBE0fFIKN91XDG6ipmNB+rOx190RbV6WxdSfQaz/5xkfP7a9crdoeQqJpIljYcvTH7L1vLtQIDAQABAoIBABsrgS7+XIkHX66Weg0rjv3kqC6PMR/6mbh0HfAF+G/laRFCd0su+hHWfM39Y5UhmPI3niVEj61zmkWnmcFe+TMgWYt3aqxkjt+JPwl4fr/Np0NVmPxiZkgQniZlwSJ9NjVZQChQ2vriOrAC+OJPcUF9PnletN+6VIOyn383W+ipY+lSfOHV/yAig8FZGS7ifMbifd9p4mXFua2onMHohSE2TK/OwTI9bfcosSRPw8XoHb+krpq9zV47ScrXAtnKPLRKFMHVITFB7BO63T/zJ1mMy5exrsFGpNJOhDhQz6M/GQrk7t2LNGO9/NbRhrd6LTN39tdXv+mK2hGzQmBkHZkCgYEA0KRRI8KLMlPVRvGMnZLaWLWoCwcMuU8Jq2l3E3eBA97d1H2Mw6kr6gkUWa/dxHgO6SDwdgZwgi93ATb0yyMKBn6/1HKAeUXDcsb7aOWt8SAX1K0qsLQ4ojvSy9nyw1IiNM1TMUQS/rvUBUzW565LL35omB8UCzCqITxnmCJOLNcCgYEA3lmUvmnn+jQTokE+h2AwpB4L7F2PtmtviecBJz/bbuXsYVHFEW6jGXHpwuWykqdOCWfS64J3XlKlLIozcOxV/C8s50FsEE/Les3ah7r0qxR/pX9COBtwEcrhajjckdnqJftgQG+0xMaEWAgWULstwfvXoPbG0cNESzY+Un18jlMCgYEAisC8NUvrxkx6SfPZz/EZxGUKnErT62jkxVoeFpQi+K/+VpIoSvb2fW4MWpjaow403FVLlTIzIGDwg55Irc9fm3IvoNmFFyGOGYa7K8eTJghx0L5Y5ar0/u9KAMew6rR0iykBaoIbH81J8zxmryz6U1t9s6z3Z3m1quTBACvQUYECgYEApgmI/IQDWaCHxNMpyVe4GuXhC3l2dsdVfEoHX3Lc/rwtPyMboP/YRYj+Aa8bIU5UBMwGAh3j24illVQCQ+IH95CW3H3LH4cmsKaF+HNQf2yIQWJ9ZX5/upmgqHyboUJ0Cjbj0lpYR9TiQQaQ+2o7Ki9Q/v8oyr0hA3UXJuxUFccCgYAsGBiofroCbfJvqeWxIZwYSNXfW0CT8RZWnpjvg+28Xb8u0MZxuHD5WDvhYoVo2ItEW8XbGXvQFBtVtEOYqn8R7HTXByuUN5LVg0+ZJM5jJ3bKhA/NtOcdcE6YEqBECjpYc0paPFebWKNhTSznPMmSzw+HR53sXVcZ1ANgMWUh8g==-----END RSA PRIVATE KEY-----"));

            var UserTenantTableHelper = new Mock<UserTenantTableHelper>();
            UserTenantTableHelper.Setup(t => t.GetUserTenantInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new TenantModel { PartitionKey = userId, RowKey= iotTenantId, Roles="[]", Timestamp = DateTime.Now }));


            this.ac = new AuthorizeController(this._config) { keyVaultHelper = keyVaultHelper.Object , UserTenantTableHelper = UserTenantTableHelper.Object};
        }

        [TestMethod]
        public async void AuthorizeControllerTokenValidAsync()
        {
            // Act
            var result = ac.PostAsync("test", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlYzk3NDVmYS05ZmE5LTRkMzUtYWI1NS0xZTNiZDA2NGM0YTIiLCJuYW1lIjoiQW5kcmV3IFNjaG1pZHQiLCJ0ZW5hbnQiOiJmMTJiMGI1OC04MDg5LTQyMmMtYTNiMS1lMWRhY2I5OTg4M2IiLCJyb2xlIjoiYWRtaW4iLCJleHAiOjE1NjI5NTQ2ODgsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjQ0MzY3IiwiYXVkIjoiSW9UUGxhdGZvcm0ifQ.TW9_QdbTCH7z-i8TWbm2AoSGNdtwS0Mj3SlASUuzuL-z82gKqn_NwxerETcYYqQqfk1lXiy59RLBurQKeUNgtxJ7pPMM8KoTNkfSjFM5-fO29geFHqNS8Mwo2sNmYmEK5fXkStkMQ6V-GrNJRx74izCQqP2rR-LRuemlYw12sDkQLDvb8Tcbx4QA4SNe5X1TmuRTwWBl08O6XG3QKHZRDoXPrANdJmgRr1HslR48edNjyHXg0TMRYfQm3WjeJtUQe9uRud54E1DmLpRcy6Kw0E6VwksFV-4bWpoi1aUV-5BesOoHw9Yi3BjUpV1zzWPHnyLsa50IF-OjR2J3T4tadQ");

            // Assert
// Assert.AreEqual(result, $"https://{keyVaultName}.vault.azure.net/secrets/{secret}");

        }
    }
}
