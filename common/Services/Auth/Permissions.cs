using System;
using System.Collections.Generic;

namespace Mmm.Platform.IoT.Common.Services.Auth
{
    public static class Permissions
    {
        public static readonly IReadOnlyDictionary<string, IEnumerable<string>> Roles = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Admin", new List<string>
                {
                    "AcquireToken",
                    "CreateDeployments",
                    "CreateDeviceGroups",
                    "CreateDevices",
                    "CreateJobs",
                    "CreatePackages",
                    "CreateRules",
                    "DeleteAlarms",
                    "DeleteDeployments",
                    "DeleteDeviceGroups",
                    "DeleteDevices",
                    "DeletePackages",
                    "DeleteRules",
                    "DeleteTenant",
                    "DisableAlerting",
                    "EnableAlerting",
                    "ReadAll",
                    "UpdateAlarms",
                    "UpdateDeviceGroups",
                    "UpdateDevices",
                    "UpdateRules",
                    "UpdateSimManagement",
                    "UserManage"
                }
            },
            {
                "ReadOnly", new List<string> 
                {
                    "ReadAll"
                }
            }
        };
    }
}
