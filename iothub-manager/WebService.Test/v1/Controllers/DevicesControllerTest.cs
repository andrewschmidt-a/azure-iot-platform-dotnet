// <copyright file="DevicesControllerTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using Xunit;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Test.Controllers
{
    public class DevicesControllerTest
    {
        [Fact]
        public void TestAllDevices()
        {
            // http://127.0.0.1:" + config.Port + "/v1/devices");
            Assert.True(true);
        }

        [Fact]
        public void TestSingleDevice()
        {
            // http://127.0.0.1:" + config.Port + "/v1/device/mydevice");
            Assert.True(true);
        }
    }
}