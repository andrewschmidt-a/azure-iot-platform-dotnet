using Microsoft.AspNetCore.Builder;

namespace Mmm.Platform.IoT.Common.WebService.Auth
{
    public interface ICorsSetup
    {
        void UseMiddleware(IApplicationBuilder app);
    }
}