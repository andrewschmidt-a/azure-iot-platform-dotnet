// <copyright file="AsaManagerClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Iot.Common.Services.Config;
using Mmm.Iot.Common.Services.Helpers;

namespace Mmm.Iot.Common.Services.External.AsaManager
{
    public class AsaManagerClient : ExternalServiceClient, IAsaManagerClient
    {
        public AsaManagerClient(AppConfig config, IExternalRequestHelper requestHelper)
            : base(config.ExternalDependencies.AsaManagerServiceUrl, requestHelper)
        {
        }

        public async Task<BeginConversionApiModel> BeginConversionAsync(string entity)
        {
            string url = $"{this.ServiceUri}/{entity}";
            return await this.RequestHelper.ProcessRequestAsync<BeginConversionApiModel>(HttpMethod.Post, url);
        }
    }
}