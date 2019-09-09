/*
The classes in this file define the required settings from app config
 */
using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime
{
    public class AppConfigSettings
    {
        // Add new keys that are necessary for this service
        public static List<string> AppConfigSettingKeys = new List<string>
        {
            "Global",
            "Global:ClientAuth",
            "Global:ClientAuth:JWT",
            "Global:AzureActiveDirectory",
            "Global:Permissions",
            "IothubManagerService",
            "IothubManagerService:DevicePropertiesCache",
            "ExternalDependencies",
            // ...
        };
    }
}
