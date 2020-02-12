// <copyright file="MockExternalClientHelper.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Mmm.Iot.Common.Services.Config;
using Mmm.Iot.Common.Services.Helpers;
using Mmm.Iot.Common.Services.Http;
using Moq;

namespace Mmm.Iot.Common.TestHelpers
{
    public class MockExternalClientHelper
    {
        public MockExternalClientHelper()
        {
            this.MockHttpClient = new Mock<IHttpClient>();

            this.MockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            this.MockHttpContextAccessor
                .Setup(t => t.HttpContext.Request.HttpContext.Items)
                .Returns(new Dictionary<object, object>() { { "TenantID", "test_tenant" } });
            this.MockHttpContextAccessor
                .Setup(t => t.HttpContext.Request.Headers)
                .Returns(new HeaderDictionary() { { this.AzdsRouteKey, "mockDevSpace" } });

            this.MockExternalRequestHelper = new Mock<IExternalRequestHelper>(
                this.MockHttpClient.Object,
                this.MockHttpContextAccessor.Object);

            this.ExternalRequestHelper = this.MockExternalRequestHelper.Object;
        }

        public string AzdsRouteKey
        {
            get
            {
                return "azds-route-as";
            }
        }

        public Mock<IHttpClient> MockHttpClient { get; private set; }

        public Mock<IHttpContextAccessor> MockHttpContextAccessor { get; private set; }

        public Mock<IExternalRequestHelper> MockIExternalRequestHelper { get; private set; }

        public IExternalRequestHelper ExternalRequestHelper { get; private set; }
    }
}