
using System;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.Azure.Management.StreamAnalytics;
using Microsoft.Azure.Management.StreamAnalytics.Models;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;
using Microsoft.Rest.Azure;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.TenantManager.Services.Helpers
{
    public class StreamAnalyticsHelper : IStreamAnalyticsHelper
    {
        private readonly AppConfig config;
        private readonly ITokenHelper _tokenHelper;

        private delegate Task<T> JobOperationDelegate<T>(string rg, string saJobName);

        public StreamAnalyticsHelper(AppConfig config, ITokenHelper tokenHelper, IAppConfigurationHelper appConfigHelper)
        {
            this.config = config;
            this._tokenHelper = tokenHelper;
        }

        private async Task<StreamAnalyticsManagementClient> GetClientAsync()
        {
            try
            {
                string authToken = "";
                try
                {
                    authToken = await this._tokenHelper.GetTokenAsync();
                    if (string.IsNullOrEmpty(authToken))
                    {
                        throw new Exception("Auth Token from tokenHelper returned a null response.");
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to get an authorization token for creating a Stream Analytics Management Client.", e);
                }

                TokenCredentials credentials = new TokenCredentials(authToken);
                StreamAnalyticsManagementClient client = new StreamAnalyticsManagementClient(credentials)
                {
                    SubscriptionId = config.Global.SubscriptionId
                };
                return client;
            }
            catch (Exception e)
            {
                throw new Exception("Unable to get a new Stream Analytics Management Client.", e);
            }
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            StreamAnalyticsManagementClient client = await this.GetClientAsync();
            return new StatusResultServiceModel(true, "Alive and well!");
        }

        public async Task<StreamingJob> GetJobAsync(string saJobName)
        {
            StreamAnalyticsManagementClient client = await this.GetClientAsync();
            try
            {
                return await client.StreamingJobs.GetAsync(config.Global.ResourceGroup, saJobName);
            }
            catch (CloudException ce)
            {
                if (ce.Message.Contains("was not found"))
                {
                    // Ensure that this cloud exception is for a resource not found exception
                    throw new ResourceNotFoundException($"The stream analytics job {saJobName} does not exist or could not be found.", ce);
                }
                else
                {
                    // otherwise, just throw the cloud exception.
                    throw ce;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An Unknown Exception occurred while attempting to get the stream analytics job {saJobName}", e);
            }
        }

        public bool JobIsActive(StreamingJob job)
        {
            return job.JobState == "Running" || job.JobState == "Starting";
        }

        public async Task StartAsync(string saJobName)
        {
            StreamAnalyticsManagementClient client = await this.GetClientAsync();
            try
            {
                await client.StreamingJobs.BeginStartAsync(config.Global.ResourceGroup, saJobName);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to start the Stream Analytics Job {saJobName}.", e);
            }
        }

        public async Task StopAsync(string saJobName)
        {
            StreamAnalyticsManagementClient client = await this.GetClientAsync();
            try
            {
                await client.StreamingJobs.BeginStopAsync(config.Global.ResourceGroup, saJobName);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to stop the Stream Analytics Job {saJobName}.", e);
            }
        }
    }
}