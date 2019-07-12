using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTTokenValidation
{

    public class CustomJwtBearerEvents : JwtBearerEvents
    {
        private IHttpContextAccessor _httpContextAccessor;
        public CustomJwtBearerEvents(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            // Add the access_token as a claim, as we may actually need it
            // Check if the user has an tenant claim
            if (!context.Principal.HasClaim(c => c.Type == "tenant"))
            {
                context.Fail($"The claim 'tenant' is not present in the token.");
            }

            Thread.CurrentPrincipal = context.Principal;
            this._httpContextAccessor.HttpContext.User = context.Principal;
            return; 
        }
        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            throw context.Exception; // throw it so it spits up in your face
            return Task.CompletedTask;
        }
    }

}