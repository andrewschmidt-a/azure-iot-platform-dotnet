// <copyright file="ClientToClientAuthMiddlewareTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Mmm.Iot.Common.Services.Auth;
using Mmm.Iot.Common.TestHelpers;
using Moq;
using Xunit;

namespace Mmm.Iot.Common.Services.Test
{
    public class ClientToClientAuthMiddlewareTest
    {
        private readonly Mock<ILogger<ClientToClientAuthMiddleware>> mockLogger;
        private readonly Mock<HttpContext> mockHttpContext;
        private readonly Mock<RequestDelegate> mockRequestDelegate;
        private readonly ClientToClientAuthMiddleware middleware;

        public ClientToClientAuthMiddlewareTest()
        {
            this.mockHttpContext = new Mock<HttpContext> { DefaultValue = DefaultValue.Mock };
            this.mockLogger = new Mock<ILogger<ClientToClientAuthMiddleware>>();
            this.mockRequestDelegate = new Mock<RequestDelegate>();

            this.mockHttpContext
                .Setup(t => t.Request.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>()
                {
                    {
                        "ApplicationTenantID",
                        "test_tenant"
                    },
                }));

            this.mockHttpContext
                .Setup(t => t.Request.SetTenant(It.IsAny<string>()));

            this.middleware = new ClientToClientAuthMiddleware(this.mockRequestDelegate.Object, this.mockLogger.Object);
        }

        [Fact]
        [Trait(Constants.Type, Constants.UnitTest)]
        public async Task SetsTenant()
        {
            await this.middleware.Invoke(this.mockHttpContext.Object);
            this.mockHttpContext.Verify(m => m.Request.SetTenant(It.IsAny<string>()), Times.Once);
        }
    }
}