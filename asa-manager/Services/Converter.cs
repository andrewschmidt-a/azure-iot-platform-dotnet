using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services.Models;
using Mmm.Platform.IoT.AsaManager.Services.Exceptions;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.AsaManager.Services
{
    public abstract class Converter : IConverter
    {
        private const string REFERENCE_DATA_DATE_FORMAT = "yyyy-MM-dd";
        private const string REFERENCE_DATA_TIME_FORMAT = "HH-mm";

        protected readonly IBlobStorageClient _blobStorageClient;
        protected readonly IStorageAdapterClient _storageAdapterClient;
        protected readonly ILogger _logger;

        public abstract string Entity { get; }
        public abstract string FileExtension { get; }

        public string dateTimeFormat = $"{REFERENCE_DATA_DATE_FORMAT}/{REFERENCE_DATA_TIME_FORMAT}";

        public Converter(
            IBlobStorageClient blobStorageClient,
            IStorageAdapterClient storageAdapterClient,
            ILogger<Converter> log)
        {
            this._blobStorageClient = blobStorageClient;
            this._storageAdapterClient = storageAdapterClient;
            this._logger = log;
        }

        public abstract Task<ConversionApiModel> ConvertAsync(string tenantId, string operationId = null);

        public string GetBlobFilePath()
        {
            string formattedDateTime = DateTimeOffset.UtcNow.ToString(this.dateTimeFormat);
            return $"{formattedDateTime}/{this.Entity}.{this.FileExtension}";
        }

        protected async Task<string> WriteFileContentToBlobAsync(string fileContent, string tenantId, string operationId = null)
        {

            if (string.IsNullOrEmpty(fileContent))
            {
                _logger.LogError("The temporary file content was null or empty for {entity}. Blank files will not be written to Blob storage. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw new BlankFileContentException($"The temporary file content serialized from the converted {this.Entity} queried from storage adapter was null or empty. Empty files will not be written to Blob storage.");
            }

            string tempFilePath = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFilePath, fileContent);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to convert {entity} to blob storage file format. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw new Exception($"Unable to convert {this.Entity} to blob storage file format.", e);
            }

            string blobFilePath = this.GetBlobFilePath();
            try
            {
                await this._blobStorageClient.CreateBlobAsync(tenantId, tempFilePath, blobFilePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to create {entity} blob for tenant. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
            }

            return blobFilePath;
        }
    }
}