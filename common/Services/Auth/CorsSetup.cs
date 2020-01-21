using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.Auth
{
    public class CorsSetup : ICorsSetup
    {
        private readonly AppConfig config;
        private readonly ILogger logger;

        public CorsSetup(AppConfig config, ILogger<CorsSetup> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public void UseMiddleware(IApplicationBuilder app)
        {
            if (config.Global.ClientAuth.CorsEnabled)
            {
                logger.LogWarning("CORS is enabled");
                app.UseCors(this.BuildCorsPolicy);
            }
            else
            {
                logger.LogInformation("CORS is disabled");
            }
        }

        private void BuildCorsPolicy(CorsPolicyBuilder builder)
        {
            CorsWhitelistModel model;
            try
            {
                model = JsonConvert.DeserializeObject<CorsWhitelistModel>(config.Global.ClientAuth.CorsWhitelist);
                if (model == null)
                {
                    logger.LogError("Ignoring invalid CORS whitelist: '{whitelist}'", config.Global.ClientAuth.CorsWhitelist);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ignoring invalid CORS whitelist: '{whitelist}'", config.Global.ClientAuth.CorsWhitelist);
                return;
            }

            if (model.Origins == null)
            {
                logger.LogInformation("No setting for CORS origin policy was found, ignore");
            }
            else if (model.Origins.Contains("*"))
            {
                logger.LogInformation("CORS policy allowed any origin");
                builder.AllowAnyOrigin();
            }
            else
            {
                logger.LogInformation("Add origins '{origins}' to CORS policy", model.Origins);
                builder.WithOrigins(model.Origins);
            }

            if (model.Origins == null)
            {
                logger.LogInformation("No setting for CORS method policy was found, ignore");
            }
            else if (model.Methods.Contains("*"))
            {
                logger.LogInformation("CORS policy allowed any method");
                builder.AllowAnyMethod();
            }
            else
            {
                logger.LogInformation("Add methods '{methods}' to CORS policy", model.Methods);
                builder.WithMethods(model.Methods);
            }

            if (model.Origins == null)
            {
                logger.LogInformation("No setting for CORS header policy was found, ignore");
            }
            else if (model.Headers.Contains("*"))
            {
                logger.LogInformation("CORS policy allowed any header");
                builder.AllowAnyHeader();
            }
            else
            {
                logger.LogInformation("Add headers '{headers}' to CORS policy", model.Headers);
                builder.WithHeaders(model.Headers);
            }
        }
    }
}
