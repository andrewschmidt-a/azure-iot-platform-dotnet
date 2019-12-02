using System.Collections.Generic;

namespace Mmm.Platform.IoT.Common.Services.Auth
{
    public interface IAuthMiddlewareConfig
    {
        Dictionary<string, List<string>> UserPermissions { get; set; }
    }
}