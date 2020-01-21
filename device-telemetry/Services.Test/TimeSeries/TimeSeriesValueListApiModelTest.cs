using System;
using System.IO;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.TestHelpers;
using Newtonsoft.Json;
using Xunit;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Test.TimeSeries
{
    public class TimeSeriesValueListApiModelTest
    {
        private readonly string tsiSampleEventsFile = $"TimeSeries{Path.DirectorySeparatorChar}TimeSeriesEvents.json";

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ConvertsToMessageList_WhenMultipleDeviceTypes()
        {
            // Arrange
            var events = this.GetTimeSeriesEvents();

            // Act
            var result = events.ToMessageList(0);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.NotEmpty(result.Properties);
            Assert.Equal(4, result.Messages.Count);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ConvertsToMessageList_WithSkipValue()
        {
            // Arrange
            var events = this.GetTimeSeriesEvents();

            // Act
            var result = events.ToMessageList(2);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.NotEmpty(result.Properties);
            Assert.Equal(2, result.Messages.Count);
        }

        private ValueListApiModel GetTimeSeriesEvents()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory +
                       Path.DirectorySeparatorChar +
                       this.tsiSampleEventsFile;

            string data = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<ValueListApiModel>(data);
        }
    }
}
