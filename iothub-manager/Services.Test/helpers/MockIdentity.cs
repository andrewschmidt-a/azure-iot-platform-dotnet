using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace Services.Test.helpers
{
    public static class MockIdentity
    {
        public static void mockClaims(string tenant)
        {
            var cp = new Mock<ClaimsPrincipal>();
            cp.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(true);

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("tenant", tenant));
            cp.Setup(m => m.Claims).Returns(claims);
            Thread.CurrentPrincipal = cp.Object;
        }
    }
}