using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services.Exceptions;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.Models;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.AsaManager.Services
{
    public abstract class Converter : IConverter
    {
        public string DateTimeFormat = $"{REFERENCE_DATA_DATE_FORMAT}/{REFERENCE_DATA_TIME_FORMAT}";
        protected readonly IBlobStorageClient blobStorageClient;
        protected readonly IStorageAdapterClient storageAdapterClient;
        protected readonly ILogger logger;
        private const string REFERENCE_DATA_DATE_FORMAT = "yyyy-MM-dd";
        private const string REFERENCE_DATA_TIME_FORMAT = "HH-mm";

        public Converter(
            IBlobStorageClient blobStorageClient,
            IStorageAdapterClient storageAdapterClient,
            ILogger<Converter> logger)
        {
            this.blobStorageClient = blobStorageClient;
            this.storageAdapterClient = storageAdapterClient;
            this.logger = logger;
        }

        public abstract string Entity { get; }

        public abstract string FileExtension { get; }

        public abstract Task<ConversionApiModel> ConvertAsync(string tenantId, string operationId = null);

        public string GetBlobFilePath()
        {
            string formattedDateTime = DateTimeOffset.UtcNow.ToString(this.DateTimeFormat);
            return $"{formattedDateTime}/{this.Entity}.{this.FileExtension}";
        }

        protected async Task<string> WriteFileContentToBlobAsync(string fileContent, string tenantId, string operationId = null)
        {

            if (string.IsNullOrEmpty(fileContent))
            {
                logger.LogError("The temporary file content was null or empty for {entity}. Blank files will not be written to Blob storage. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw new BlankFileContentException($"The temporary file content serialized from the converted {this.Entity} queried from storage adapter was null or empty. Empty files will not be written to Blob storage.");
            }

            string tempFilePath = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFilePath, fileContent);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to convert {entity} to blob storage file format. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw new Exception($"Unable to convert {this.Entity} to blob storage file format.", e);
            }

            string blobFilePath = this.GetBlobFilePath();
            try
            {
                await this.blobStorageClient.CreateBlobAsync(tenantId, tempFilePath, blobFilePath);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to create {entity} blob for tenant. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
            }

            return blobFilePath;
        }
    }
}