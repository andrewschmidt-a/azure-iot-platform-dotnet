using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Mmm.Platform.IoT.Config.Services.External;
using Mmm.Platform.IoT.Config.Services.Helpers.PackageValidation;
using Mmm.Platform.IoT.Config.Services.Models;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mmm.Platform.IoT.Common.Services.Config;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNetCore.Http;
using Mmm.Platform.IoT.Common.Services;

namespace Mmm.Platform.IoT.Config.Services
{
    public class Storage : IStorage
    {
        private readonly IStorageAdapterClient _client;
        private readonly IAsaManagerClient _asaManager;
        private readonly AppConfig config;
        private readonly ILogger _logger;
        public const string SOLUTION_COLLECTION_ID = "solution-settings";
        public const string THEME_KEY = "theme";
        public const string LOGO_KEY = "logo";
        public const string USER_COLLECTION_ID = "user-settings";
        public const string DEVICE_GROUP_COLLECTION_ID = "devicegroups";
        public const string PACKAGES_COLLECTION_ID = "packages";
        public const string PACKAGES_CONFIG_TYPE_KEY = "config-types";
        public const string AZURE_MAPS_KEY = "AzureMapsKey";
        public const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";
        public const string SOFTWARE_PACKAGE_STORE = "software-package";

        public Storage(
            IStorageAdapterClient client,
            IAsaManagerClient asaManager,
            AppConfig config,
            ILogger<Storage> logger)
        {
            this._client = client;
            this._asaManager = asaManager;
            this.config = config;
            this._logger = logger;
        }
        public async Task<object> GetThemeAsync()
        {
            string data;

            try
            {
                var response = await this._client.GetAsync(SOLUTION_COLLECTION_ID, THEME_KEY);
                data = response.Data;
            }
            catch (ResourceNotFoundException)
            {
                data = JsonConvert.SerializeObject(Theme.Default);
            }

            var themeOut = JsonConvert.DeserializeObject(data) as JToken ?? new JObject();
            this.AppendAzureMapsKey(themeOut);
            return themeOut;
        }

        public async Task<object> SetThemeAsync(object themeIn)
        {
            var value = JsonConvert.SerializeObject(themeIn);
            var response = await this._client.UpdateAsync(SOLUTION_COLLECTION_ID, THEME_KEY, value, "*");
            var themeOut = JsonConvert.DeserializeObject(response.Data) as JToken ?? new JObject();
            this.AppendAzureMapsKey(themeOut);
            return themeOut;
        }

        private void AppendAzureMapsKey(JToken theme)
        {
            if (theme[AZURE_MAPS_KEY] == null)
            {
                theme[AZURE_MAPS_KEY] = config.ConfigService.AzureMapsKey;
            }
        }

        public async Task<object> GetUserSetting(string id)
        {
            try
            {
                var response = await this._client.GetAsync(USER_COLLECTION_ID, id);
                return JsonConvert.DeserializeObject(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                return new object();
            }
        }

        public async Task<object> SetUserSetting(string id, object setting)
        {
            var value = JsonConvert.SerializeObject(setting);
            var response = await this._client.UpdateAsync(USER_COLLECTION_ID, id, value, "*");
            return JsonConvert.DeserializeObject(response.Data);
        }

        public async Task<Logo> GetLogoAsync()
        {
            try
            {
                var response = await this._client.GetAsync(SOLUTION_COLLECTION_ID, LOGO_KEY);
                return JsonConvert.DeserializeObject<Logo>(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                return Logo.Default;
            }
        }

        public async Task<Logo> SetLogoAsync(Logo model)
        {
            //Do not overwrite existing name or image with null
            if (model.Name == null || model.Image == null)
            {
                Logo current = await this.GetLogoAsync();
                if (!current.IsDefault)
                {
                    model.Name = model.Name ?? current.Name;
                    if (model.Image == null && current.Image != null)
                    {
                        model.Image = current.Image;
                        model.Type = current.Type;
                    }
                }
            }

            var value = JsonConvert.SerializeObject(model);
            var response = await this._client.UpdateAsync(SOLUTION_COLLECTION_ID, LOGO_KEY, value, "*");
            return JsonConvert.DeserializeObject<Logo>(response.Data);
        }

        public async Task<IEnumerable<DeviceGroup>> GetAllDeviceGroupsAsync()
        {
            var response = await this._client.GetAllAsync(DEVICE_GROUP_COLLECTION_ID);
            return response.Items.Select(this.CreateGroupServiceModel);
        }

        public async Task<DeviceGroup> GetDeviceGroupAsync(string id)
        {
            var response = await this._client.GetAsync(DEVICE_GROUP_COLLECTION_ID, id);
            return this.CreateGroupServiceModel(response);
        }

        public async Task<DeviceGroup> CreateDeviceGroupAsync(DeviceGroup input)
        {
            var value = JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var response = await this._client.CreateAsync(DEVICE_GROUP_COLLECTION_ID, value);
            var responseModel = this.CreateGroupServiceModel(response);
            await this._asaManager.BeginConversionAsync(DEVICE_GROUP_COLLECTION_ID);
            return responseModel;
        }

        public async Task<DeviceGroup> UpdateDeviceGroupAsync(string id, DeviceGroup input, string etag)
        {
            var value = JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var response = await this._client.UpdateAsync(DEVICE_GROUP_COLLECTION_ID, id, value, etag);
            await this._asaManager.BeginConversionAsync(DEVICE_GROUP_COLLECTION_ID);
            return this.CreateGroupServiceModel(response);
        }

        public async Task DeleteDeviceGroupAsync(string id)
        {
            await this._client.DeleteAsync(DEVICE_GROUP_COLLECTION_ID, id);
            await this._asaManager.BeginConversionAsync(DEVICE_GROUP_COLLECTION_ID);
        }

        public async Task<IEnumerable<PackageServiceModel>> GetAllPackagesAsync()
        {
            var response = await this._client.GetAllAsync(PACKAGES_COLLECTION_ID);
            return response.Items.AsParallel().Where(r => r.Key != PACKAGES_CONFIG_TYPE_KEY)
                .Select(this.CreatePackageServiceModel);
        }

        public async Task<IEnumerable<PackageServiceModel>> GetFilteredPackagesAsync(string packageType, string configType)
        {
            var response = await this._client.GetAllAsync(PACKAGES_COLLECTION_ID);
            IEnumerable<PackageServiceModel> packages = response.Items.AsParallel()
                .Where(r => r.Key != PACKAGES_CONFIG_TYPE_KEY)
                .Select(this.CreatePackageServiceModel);

            bool isPackageTypeEmpty = string.IsNullOrEmpty(packageType);
            bool isConfigTypeEmpty = string.IsNullOrEmpty(configType);

            if (!isPackageTypeEmpty && !isConfigTypeEmpty)
            {
                return packages.Where(p => (p.PackageType.ToString().Equals(packageType) &&
                                           p.ConfigType.Equals(configType)));
            }
            else if (!isPackageTypeEmpty && isConfigTypeEmpty)
            {
                return packages.Where(p => p.PackageType.ToString().Equals(packageType));
            }
            else if (isPackageTypeEmpty && !isConfigTypeEmpty)
            {
                // Non-empty ConfigType with empty PackageType indicates invalid packages
                throw new InvalidInputException("Package Type cannot be empty.");
            }
            else
            {
                // Return all packages when ConfigType & PackageType are empty
                return packages;
            }
        }

        public async Task<PackageServiceModel> AddPackageAsync(PackageServiceModel package)
        {
            bool isValidPackage = IsValidPackage(package);
            if (!isValidPackage)
            {
                var msg = "Package provided is a invalid deployment manifest " +
                    $"for type {package.PackageType}";

                msg += package.PackageType.Equals(PackageType.DeviceConfiguration) ?
                    $"and configuration {package.ConfigType}" : string.Empty;

                throw new InvalidInputException(msg);
            }

            try
            {
                JsonConvert.DeserializeObject<Configuration>(package.Content);
            }
            catch (Exception)
            {
                throw new InvalidInputException("Package provided is not a valid deployment manifest");
            }

            package.DateCreated = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
            var value = JsonConvert.SerializeObject(package,
                                                    Formatting.Indented,
                                                    new JsonSerializerSettings
                                                    {
                                                        NullValueHandling = NullValueHandling.Ignore
                                                    });

            var response = await this._client.CreateAsync(PACKAGES_COLLECTION_ID, value);

            if (!(string.IsNullOrEmpty(package.ConfigType))
                && package.PackageType.Equals(PackageType.DeviceConfiguration))
            {
                await this.UpdateConfigTypeAsync(package.ConfigType);
            }

            return this.CreatePackageServiceModel(response);
        }

        public async Task DeletePackageAsync(string id)
        {
            await this._client.DeleteAsync(PACKAGES_COLLECTION_ID, id);
        }

        public async Task<PackageServiceModel> GetPackageAsync(string id)
        {
            var response = await this._client.GetAsync(PACKAGES_COLLECTION_ID, id);
            return this.CreatePackageServiceModel(response);
        }

        public async Task<ConfigTypeListServiceModel> GetConfigTypesListAsync()
        {
            try
            {
                var response = await this._client.GetAsync(PACKAGES_COLLECTION_ID, PACKAGES_CONFIG_TYPE_KEY);
                return JsonConvert.DeserializeObject<ConfigTypeListServiceModel>(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                _logger.LogDebug("Document config-types has not been created.");
                // Return empty Package Config types
                return new ConfigTypeListServiceModel();
            }
        }

        public async Task UpdateConfigTypeAsync(string customConfigType)
        {
            ConfigTypeListServiceModel list;
            try
            {
                var response = await this._client.GetAsync(PACKAGES_COLLECTION_ID, PACKAGES_CONFIG_TYPE_KEY);
                list = JsonConvert.DeserializeObject<ConfigTypeListServiceModel>(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                _logger.LogDebug("Config Types have not been created.");
                // Create empty Package Config Types
                list = new ConfigTypeListServiceModel();
            }
            list.add(customConfigType);
            await this._client.UpdateAsync(PACKAGES_COLLECTION_ID, PACKAGES_CONFIG_TYPE_KEY, JsonConvert.SerializeObject(list), "*");
        }

        public async Task<string> UploadToBlobAsync(string tenantId, string filename, Stream stream = null)
        {
            CloudStorageAccount storageAccount = null;
            CloudBlobContainer cloudBlobContainer = null;
            string url = string.Empty;
            string storageConnectionString = config.Global.StorageAccountConnectionString;
            string  duration = config.Global.PackageSharedAccessExpiryTime;

            if(string.IsNullOrEmpty(tenantId))
            {
                _logger.LogError("Tenant ID is blank, cannot create container without tenandId.");
                return null;
            }

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a container 
                    cloudBlobContainer = cloudBlobClient.GetContainerReference($"{tenantId}-{SOFTWARE_PACKAGE_STORE}");
                    
                    // Create the container if it does not already exist
                    await cloudBlobContainer.CreateIfNotExistsAsync();

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(filename);

                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        _logger.LogError("Empty stream object in the UploadToBlob method.");
                        return null;
                    }
                    url = Convert.ToString(GetBlobSasUri(cloudBlobClient, cloudBlobContainer.Name, filename, duration));
                    return url;
                }
                catch (StorageException ex)
                {
                    _logger.LogError($"Exception in the UploadToBlob method- Message: {ex.Message} : Stack Trace - {ex.StackTrace.ToString()}");
                    return null;
                }
            }
            else
            {
                _logger.LogError("Error parsing CloudStorageAccount in UploadToBlob method");
                return null;
            }
        }

        private string GetBlobSasUri(CloudBlobClient cloudBlobClient,string containerName, string blobName, string timeoutDuration)
        {
            string[] time = timeoutDuration.Split(':');
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(Convert.ToDouble(time[0])).AddMinutes(Convert.ToDouble(time[1])).AddSeconds(Convert.ToDouble(time[2]));
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blockBlob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blockBlob.Uri + sasBlobToken;
        }

        private Boolean IsValidPackage(PackageServiceModel package)
        {
            IPackageValidator validator = PackageValidatorFactory.GetValidator(
                package.PackageType,
                package.ConfigType);

            // Bypass validation for custom _config type
            return validator == null || validator.Validate();
        }

        private DeviceGroup CreateGroupServiceModel(ValueApiModel input)
        {
            var output = JsonConvert.DeserializeObject<DeviceGroup>(input.Data);
            output.Id = input.Key;
            output.ETag = input.ETag;
            return output;
        }

        private PackageServiceModel CreatePackageServiceModel(ValueApiModel input)
        {
            var output = JsonConvert.DeserializeObject<PackageServiceModel>(input.Data);
            output.Id = input.Key;
            return output;
        }
    }
}
