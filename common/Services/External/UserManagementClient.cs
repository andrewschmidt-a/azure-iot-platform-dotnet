// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.Common.Services.External
{
    public class UserManagementClient : ExternalServiceClient, IUserManagementClient
    {
        private const string DEFAULT_USER_ID = "default";

        public UserManagementClient(
            AppConfig config,
            IExternalRequestHelper requestHelper) :
            base(config.ExternalDependencies.AuthServiceUrl, requestHelper)
        {
        }

        public async Task<IEnumerable<string>> GetAllowedActionsAsync(string userObjectId, IEnumerable<string> roles)
        {
            string url = $"{this.serviceUri}/users/{userObjectId}/allowedActions";
            return await this._requestHelper.ProcessRequestAsync<IEnumerable<string>>(HttpMethod.Post, url, roles);
        }

        public async Task<string> GetTokenAsync()
        {
            // Note: The DEFAULT_USER_ID is set to any value. The user management service doesn't 
            // currently use the user ID information, but if this API is updated in the future, we 
            // will need to grab the user ID from the request JWT token and pass in here.
            string url = $"{this.serviceUri}/users/{DEFAULT_USER_ID}/token";
            TokenApiModel tokenModel = await this._requestHelper.ProcessRequestAsync<TokenApiModel>(HttpMethod.Get, url);
            return tokenModel.AccessToken;
        }
    }
}