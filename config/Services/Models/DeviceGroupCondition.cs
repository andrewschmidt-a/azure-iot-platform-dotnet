// Copyright (c) Microsoft. All rights reserved.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mmm.Platform.IoT.Config.Services.Models
{
    public class DeviceGroupCondition
    {
        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("Operator")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OperatorType Operator { get; set; }

        [JsonProperty("Value")]
        public object Value { get; set; }
    }

    public enum OperatorType
    {
        // ReSharper disable once InconsistentNaming
        EQ, // =
        // ReSharper disable once InconsistentNaming
        NE, // !=
        // ReSharper disable once InconsistentNaming
        LT, // <
        // ReSharper disable once InconsistentNaming
        GT, // >
        // ReSharper disable once InconsistentNaming
        LE, // <=
        // ReSharper disable once InconsistentNaming
        GE, // >=
        // ReSharper disable once InconsistentNaming
        IN // IN
    }
}
