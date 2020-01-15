using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.Models;
using Mmm.Platform.IoT.AsaManager.Services.Models.Rules;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Exceptions;

namespace Mmm.Platform.IoT.AsaManager.Services
{
    public class RulesConverter : Converter, IConverter
    {
        public override string Entity { get { return "rules"; } }
        public override string FileExtension { get { return "json"; } }

        public RulesConverter(
            IBlobStorageClient blobClient,
            IStorageAdapterClient storageAdapterClient,
            ILogger<RulesConverter> log) : base(blobClient, storageAdapterClient, log)
        {
        }

        public override async Task<ConversionApiModel> ConvertAsync(string tenantId, string operationId = null)
        {
            ValueListApiModel rules = null;
            try
            {
                rules = await this._storageAdapterClient.GetAllAsync(this.Entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to query {entity} using storage adapter. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw e;
            }
            if (rules.Items.Count() == 0 || rules == null)
            {
                _logger.LogError("No entities were receieved from storage adapter to convert to {entity}. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw new ResourceNotFoundException("No entities were receieved from storage adapter to convert to rules.");
            }

            List<RuleReferenceDataModel> jsonRulesList = new List<RuleReferenceDataModel>();
            try
            {
                foreach (ValueApiModel item in rules.Items)
                {
                    try
                    {
                        RuleDataModel dataModel = JsonConvert.DeserializeObject<RuleDataModel>(item.Data);
                        RuleModel ruleModel = new RuleModel(item.Key, dataModel);
                        // return a RuleReferenceModel which is a conversion from the RuleModel into a SAjob readable format with additional metadata
                        RuleReferenceDataModel referenceModel = new RuleReferenceDataModel(ruleModel);
                        jsonRulesList.Add(referenceModel);
                    }
                    catch (Exception)
                    {
                        _logger.LogInformation("Unable to convert a rule to the proper reference data model for {entity}. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                    }
                }
                if (jsonRulesList.Count() == 0)
                {
                    throw new ResourceNotSupportedException("No rules were able to be converted to the proper rule reference data model.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to convert {entity} queried from storage adapter to appropriate data model. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw e;
            }

            string fileContent = null;
            try
            {
                fileContent = JsonConvert.SerializeObject(jsonRulesList, Formatting.Indented);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to serialize the IEnumerable of {entity} data models for the temporary file content. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw e;
            }

            string blobFilePath = await this.WriteFileContentToBlobAsync(fileContent, tenantId, operationId);

            ConversionApiModel conversionResponse = new ConversionApiModel
            {
                TenantId = tenantId,
                BlobFilePath = blobFilePath,
                Entities = rules,
                OperationId = operationId
            };
            _logger.LogInformation("Successfully Completed {entity} conversion\n{model}", this.Entity, JsonConvert.SerializeObject(conversionResponse));
            return conversionResponse;
        }
    }
}