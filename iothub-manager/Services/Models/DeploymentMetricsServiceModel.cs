using System.Collections.Generic;
using Microsoft.Azure.Devices;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Models
{
    public class DeploymentMetricsServiceModel
    {
        public DeploymentMetricsServiceModel(
            ConfigurationMetrics systemMetrics,
            ConfigurationMetrics customMetrics)
        {
            this.SystemMetrics = systemMetrics?.Results;
            this.CustomMetrics = customMetrics?.Results;
        }

        public IDictionary<string, long> SystemMetrics { get; set; }
        public IDictionary<string, long> CustomMetrics { get; set; }
        public IDictionary<DeploymentStatus, long> DeviceMetrics { get; set; }
        public IDictionary<string, DeploymentStatus> DeviceStatuses { get; set; }
    }
}
