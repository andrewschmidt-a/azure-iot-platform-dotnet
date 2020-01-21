// Copyright (c) Microsoft. All rights reserved.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.TenantManager.Services.Models
{
    public class DeviceGroupApiModel
    {
        public string DisplayName { get; set; }

        public IEnumerable<DeviceGroupConditionModel> Conditions { get; set; }
    }
}