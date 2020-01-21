// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    public class DeploymentServiceListModel
    {
        public DeploymentServiceListModel(List<DeploymentServiceModel> items)
        {
            this.Items = items;
        }

        public List<DeploymentServiceModel> Items { get; set; }
    }
}
