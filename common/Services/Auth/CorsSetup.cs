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
        private readonly ILogger _logger;

        public CorsSetup(AppConfig config, ILogger<CorsSetup> logger)
        {
            this.config = config;
            _logger = logger;
        }

        public void UseMiddleware(IApplicationBuilder app)
        {
            if (config.Global.ClientAuth.CorsEnabled)
            {
                _logger.LogWarning("CORS is enabled");
                app.UseCors(this.BuildCorsPolicy);
            }
            else
            {
                _logger.LogInformation("CORS is disabled");
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
                    _logger.LogError("Ignoring invalid CORS whitelist: '{whitelist}'", config.Global.ClientAuth.CorsWhitelist);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ignoring invalid CORS whitelist: '{whitelist}'", config.Global.ClientAuth.CorsWhitelist);
                return;
            }

            if (model.Origins == null)
            {
                _logger.LogInformation("No setting for CORS origin policy was found, ignore");
            }
            else if (model.Origins.Contains("*"))
            {
                _logger.LogInformation("CORS policy allowed any origin");
                builder.AllowAnyOrigin();
            }
            else
            {
                _logger.LogInformation("Add origins '{origins}' to CORS policy", model.Origins);
                builder.WithOrigins(model.Origins);
            }

            if (model.Origins == null)
            {
                _logger.LogInformation("No setting for CORS method policy was found, ignore");
            }
            else if (model.Methods.Contains("*"))
            {
                _logger.LogInformation("CORS policy allowed any method");
                builder.AllowAnyMethod();
            }
            else
            {
                _logger.LogInformation("Add methods '{methods}' to CORS policy", model.Methods);
                builder.WithMethods(model.Methods);
            }

            if (model.Origins == null)
            {
                _logger.LogInformation("No setting for CORS header policy was found, ignore");
            }
            else if (model.Headers.Contains("*"))
            {
                _logger.LogInformation("CORS policy allowed any header");
                builder.AllowAnyHeader();
            }
            else
            {
                _logger.LogInformation("Add headers '{headers}' to CORS policy", model.Headers);
                builder.WithHeaders(model.Headers);
            }
        }
    }
}
