using System.Collections.Generic;

namespace Mmm.Platform.IoT.Common.WebService.Auth
{
    public interface IAuthMiddlewareConfig
    {
        Dictionary<string, List<string>> UserPermissions { get; set; }
    }
}