// <copyright file="IotHubManagerClientTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.AsaManager.Services.Exceptions;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups;
using Mmm.Platform.IoT.AsaManager.Services.Test.Helpers;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Mmm.Platform.IoT.AsaManager.Services.Test
{
    public class IothubmanagerclientTest
    {
        private const string MockApiUrl = "http://iothub:80/v1";
        private Mock<AppConfig> mockConfig;
        private Mock<IExternalRequestHelper> mockRequestHelper;
        private IIotHubManagerClient client;
        private Random rand;
        private CreateEntityHelper entityHelper;

        public IothubmanagerclientTest()
        {
            this.mockConfig = new Mock<AppConfig>();
            this.mockConfig
                .Setup(c => c.ExternalDependencies.IotHubManagerServiceUrl)
                .Returns(MockApiUrl);

            this.mockRequestHelper = new Mock<IExternalRequestHelper>();

            this.client = new IotHubManagerClient(this.mockConfig.Object, this.mockRequestHelper.Object);
            this.rand = new Random();
            this.entityHelper = new CreateEntityHelper(this.rand);
        }

        [Fact]
        public async Task GetListAsyncReturnsExpectedValue()
        {
            string tenantId = this.rand.NextString();
            List<DeviceGroupConditionModel> conditions = new List<DeviceGroupConditionModel>();
            List<DeviceModel> devices = new List<DeviceModel>
            {
                this.entityHelper.CreateDevice(),
                this.entityHelper.CreateDevice(),
            };
            DeviceListModel deviceListModel = new DeviceListModel
            {
                Items = devices,
            };

            this.mockRequestHelper
                .Setup(r => r.ProcessRequestAsync<DeviceListModel>(
                    It.Is<HttpMethod>(m => m == HttpMethod.Get),
                    It.Is<string>(url => url.Contains(MockApiUrl)),
                    It.Is<string>(s => s == tenantId)))
                .ReturnsAsync(deviceListModel);

            DeviceListModel response = await this.client.GetListAsync(conditions, tenantId);

            this.mockRequestHelper
                .Verify(
                    r => r.ProcessRequestAsync<DeviceListModel>(
                        It.Is<HttpMethod>(m => m == HttpMethod.Get),
                        It.Is<string>(url => url.Contains(MockApiUrl)),
                        It.Is<string>(s => s == tenantId)),
                    Times.Once);

            Assert.Equal(deviceListModel, response);
        }
    }
}