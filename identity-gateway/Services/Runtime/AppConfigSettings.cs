/*
The classes in this file define the required settings from app config
 */
using System.Collections.Generic;

namespace IdentityGateway.Services.Runtime
{
    public class AppConfigSettings
    {
        // Add the parent keys for each required key (does not grab children-of-children keys)
        public static List<string> AppConfigSettingKeys = new List<string>
        {
            "Global",
            "Global:AzureActiveDirectory",
            // ...
        };
    }
}
