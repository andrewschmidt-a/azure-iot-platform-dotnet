/*
The classes in this file define the required settings from app config
 */
using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public class AppConfigSettings
    {
        public static List<string> AppConfigSettingKeys = new List<string>
        {
            "Global",
            "Global:ClientAuth",
            "Global:ClientAuth:JWT",
            "Global:AzureActiveDirectory",
            "Global:Permissions",
            "TelemetryService",
            "TelemetryService:TimeSeries",
            "TelemetryService:CosmosDb",
            "TelemetryService:Messages",
            "TelemetryService:Alarms",
            "ExternalDependencies",
            "Actions"
        };
    }
}