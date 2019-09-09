/*
The classes in this file define the required settings from app config
 */
using System.Collections.Generic;
namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime
{
    public class AppConfigSettings
    {
        public static List<string> AppConfigSettingKeys = new List<string>
        {
            "Global",
            "Global:ClientAuth",
            "Global:ClientAuth:JWT",
            "Global:Permissions",
            "ConfigService",
            "ConfigService:Actions",
            "ExternalDependencies",
            "UserManagementService"
        };
    }
}
