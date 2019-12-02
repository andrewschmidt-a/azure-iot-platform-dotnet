using Microsoft.AspNetCore.Builder;

namespace Mmm.Platform.IoT.Common.Services.Auth
{
    public interface ICorsSetup
    {
        void UseMiddleware(IApplicationBuilder app);
    }
}