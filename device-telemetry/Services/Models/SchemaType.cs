// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SchemaType
    {
        TelemetryAgent = 0,
        StreamingJobs = 1,
    }
}
