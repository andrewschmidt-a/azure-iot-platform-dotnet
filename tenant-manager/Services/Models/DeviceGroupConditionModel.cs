// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Models
{
    public class DeviceGroupConditionModel
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }

        public DeviceGroupConditionModel()
        {
        }
    }
}
