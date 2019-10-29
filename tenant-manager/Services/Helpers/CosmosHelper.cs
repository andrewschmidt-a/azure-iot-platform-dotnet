using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Models;
using MMM.Azure.IoTSolutions.TenantManager.Services.Runtime;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Helpers
{
    public class CosmosHelper : IStatusOperation
    {
        private DocumentClient client;

        public CosmosHelper(IServicesConfig config)
        {
            try
            {
                this.client = new DocumentClient(new Uri(config.CosmosDbEndpoint), config.CosmosDbToken);
            }
            catch (Exception e)
            {
                throw new InvalidConfigurationException("Unable to create CosmosDb DocumentClient with the given Cosmos Key & Token. Check to ensure they are configured correctly.", e);
            }
        }

        public async Task<StatusResultServiceModel> StatusAsync()
        {
            if (this.client != null)
            {
                // make generic call to see if storage client can be reached
                try
                {
                    // ping the client for a db account
                    await this.client.GetDatabaseAccountAsync();
                }
                catch (Exception e)
                {
                    return new StatusResultServiceModel(false, $"Storage check failed: {e.Message}");
                }
            }
            else
            {
                return new StatusResultServiceModel(false, "Storage check failed.");
            }

            // The ping was successful - the service is running as intended
            return new StatusResultServiceModel(true, "Alive and Well!");
        }

        public async Task DeleteCosmosDbCollection(string database, string collectionId)
        {
            try
            {
                await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(database, collectionId));
            }
            catch (DocumentClientException dce)
            {
                string message = "";
                if (dce.Message.Contains("Resource Not Found"))
                {
                    // status code 404 for the DocumentClientException means that the colleciton does not exist
                    message = $"The {collectionId} collection does not exist and cannot be deleted.";
                }
                else
                {
                    message = $"An error occurred while deleting the {collectionId} collection.";
                }
                throw new ResourceNotFoundException(message, dce);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while deleting the {collectionId} collection.", e);  // Throw the same exception thrown by the cosmos client
            }
        }

        public async Task CreateCosmosDbCollection(string database, string collectionId)
        {
            try
            {
                DocumentCollection collection = new DocumentCollection { Id = collectionId, PartitionKey = new PartitionKeyDefinition { Paths = new Collection<string> { "/_deviceId" } } };
                await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(database), collection);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to create cosmosDb collection {collectionId}", e);  // Throw the same exception thrown by the cosmos client
            }
        }
    }
}