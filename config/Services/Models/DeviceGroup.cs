// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Mmm.Platform.IoT.Config.Services.Models
{
    public class DeviceGroup
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<DeviceGroupCondition> Conditions { get; set; }
        public string ETag { get; set; }
    }
}
