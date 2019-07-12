
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace IoTTokenValidation
{
    /// <summary>
    /// DI extension methods for adding IoT Jwt Token Validation
    /// </summary>
    public static class IServiceCollectionExtension
    {

        /// <summary>
        /// Adds Auth and Validator
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddIoTTokenValidator(this IServiceCollection services, IoTTokenValidatorOptions optionsIn)
        {
            services.AddScoped<CustomJwtBearerEvents>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = optionsIn.Authority;
                    options.Audience = "IoTPlatform";
                    options.RequireHttpsMetadata = false;
                    options.EventsType = typeof(CustomJwtBearerEvents);
                });
            return services;
        }

    }
    public class IoTTokenValidatorOptions
    {
        public string Authority; 
    }

}
