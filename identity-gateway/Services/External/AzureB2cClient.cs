// <copyright file="AzureB2cClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.IdentityGateway.Services.External
{
    public class AzureB2cClient : IAzureB2cClient
    {
        private readonly string serviceUri;

        private readonly IExternalRequestHelper requestHelper;

        public AzureB2cClient(
            AppConfig config,
            IExternalRequestHelper requestHelper)
        {
            this.serviceUri = config.Global.AzureB2cBaseUri;
            this.requestHelper = requestHelper;
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            try
            {
                var response = await this.requestHelper.ProcessRequestAsync(HttpMethod.Get, this.serviceUri);
                if (response.IsSuccessStatusCode)
                {
                    return new StatusResultServiceModel(response.IsSuccessStatusCode, "Alive and well!");
                }
                else
                {
                    return new StatusResultServiceModel(false, $"AzureB2C status check failed with code {response.StatusCode}.");
                }
            }
            catch (Exception e)
            {
                return new StatusResultServiceModel(false, e.Message);
            }
        }
    }
}