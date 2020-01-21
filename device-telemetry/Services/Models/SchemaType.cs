// <copyright file="SchemaType.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

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