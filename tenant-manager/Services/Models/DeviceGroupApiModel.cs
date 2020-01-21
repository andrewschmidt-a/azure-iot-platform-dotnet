// <copyright file="DeviceGroupApiModel.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace Mmm.Platform.IoT.TenantManager.Services.Models
{
    public class DeviceGroupApiModel
    {
        public string DisplayName { get; set; }

        public IEnumerable<DeviceGroupConditionModel> Conditions { get; set; }
    }
}