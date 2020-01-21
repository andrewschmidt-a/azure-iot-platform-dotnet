using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.IoTHubManager.Services.Helpers;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public class DeviceProperties : IDeviceProperties
    {
        public const string CacheCollectioId = "device-twin-properties";
        public const string CacheKey = "cache";
        private const string WhitelistTagPrefix = "tags.";
        private const string WhitelistReportedPrefix = "reported.";
        private const string TagPrefix = "Tags.";
        private const string ReportedPrefix = "Properties.Reported.";
        private readonly IStorageAdapterClient storageClient;
        private readonly IDevices devices;
        private readonly ILogger logger;
        private readonly string whitelist;
        private readonly long ttl;
        private readonly long rebuildTimeout;
        private readonly TimeSpan serviceQueryInterval = TimeSpan.FromSeconds(10);
        private DateTime devicePropertiesLastUpdated;

        public DeviceProperties(
            IStorageAdapterClient storageClient,
            AppConfig config,
            ILogger<DeviceProperties> logger,
            IDevices devices)
        {
            this.storageClient = storageClient;
            this.logger = logger;
            this.whitelist = config.IotHubManagerService.DevicePropertiesCache.Whitelist;
            this.ttl = config.IotHubManagerService.DevicePropertiesCache.Ttl;
            this.rebuildTimeout = config.IotHubManagerService.DevicePropertiesCache.RebuildTimeout;
            this.devices = devices;
        }

        public async Task<List<string>> GetListAsync()
        {
            ValueApiModel response = new ValueApiModel();
            try
            {
                response = await this.storageClient.GetAsync(CacheCollectioId, CacheKey);
            }
            catch (ResourceNotFoundException)
            {
                logger.LogDebug($"Cache get: cache {CacheCollectioId}:{CacheKey} was not found");
            }
            catch (Exception e)
            {
                throw new ExternalDependencyException(
                    $"Cache get: unable to get device-twin-properties cache", e);
            }

            if (string.IsNullOrEmpty(response?.Data))
            {
                throw new Exception($"StorageAdapter did not return any data for {CacheCollectioId}:{CacheKey}. The DeviceProperties cache has not been created for this tenant yet.");
            }

            DevicePropertyServiceModel properties = new DevicePropertyServiceModel();
            try
            {
                properties = JsonConvert.DeserializeObject<DevicePropertyServiceModel>(response.Data);
            }
            catch (Exception e)
            {
                throw new InvalidInputException("Unable to deserialize deviceProperties from CosmosDB", e);
            }

            List<string> result = new List<string>();
            foreach (string tag in properties.Tags)
            {
                result.Add(TagPrefix + tag);
            }

            foreach (string reported in properties.Reported)
            {
                result.Add(ReportedPrefix + reported);
            }

            return result;
        }

        public async Task<bool> TryRecreateListAsync(bool force = false)
        {
            var @lock = new StorageWriteLock<DevicePropertyServiceModel>(
                this.storageClient,
                CacheCollectioId,
                CacheKey,
                (c, b) => c.Rebuilding = b,
                m => this.ShouldCacheRebuild(force, m));

            while (true)
            {
                var locked = await @lock.TryLockAsync();
                if (locked == null)
                {
                    logger.LogWarning("Cache rebuilding: lock failed due to conflict. Retry soon");
                    continue;
                }

                if (!locked.Value)
                {
                    return false;
                }

                // Build the cache content
                var twinNamesTask = this.GetValidNamesAsync();

                try
                {
                    Task.WaitAll(twinNamesTask);
                }
                catch (Exception)
                {
                    logger.LogWarning("Some underlying service is not ready. Retry after {interval}.", this.serviceQueryInterval);
                    try
                    {
                        await @lock.ReleaseAsync();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Cache rebuilding: Unable to release lock");
                    }

                    await Task.Delay(this.serviceQueryInterval);
                    continue;
                }

                var twinNames = twinNamesTask.Result;
                try
                {
                    var updated = await @lock.WriteAndReleaseAsync(
                        new DevicePropertyServiceModel
                        {
                            Tags = twinNames.Tags,
                            Reported = twinNames.ReportedProperties,
                        });
                    if (updated)
                    {
                        this.devicePropertiesLastUpdated = DateTime.Now;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Cache rebuilding: Unable to write and release lock");
                }

                logger.LogWarning("Cache rebuilding: write failed due to conflict. Retry soon");
            }
        }

        public async Task<DevicePropertyServiceModel> UpdateListAsync(
            DevicePropertyServiceModel deviceProperties)
        {
            // To simplify code, use empty set to replace null set
            deviceProperties.Tags = deviceProperties.Tags ?? new HashSet<string>();
            deviceProperties.Reported = deviceProperties.Reported ?? new HashSet<string>();

            string etag = null;
            while (true)
            {
                ValueApiModel model = null;
                try
                {
                    model = await this.storageClient.GetAsync(CacheCollectioId, CacheKey);
                }
                catch (ResourceNotFoundException)
                {
                    logger.LogInformation($"Cache updating: cache {CacheCollectioId}:{CacheKey} was not found");
                }

                if (model != null)
                {
                    DevicePropertyServiceModel devicePropertiesFromStorage;

                    try
                    {
                        devicePropertiesFromStorage = JsonConvert.
                            DeserializeObject<DevicePropertyServiceModel>(model.Data);
                    }
                    catch
                    {
                        devicePropertiesFromStorage = new DevicePropertyServiceModel();
                    }

                    devicePropertiesFromStorage.Tags = devicePropertiesFromStorage.Tags ??
                        new HashSet<string>();
                    devicePropertiesFromStorage.Reported = devicePropertiesFromStorage.Reported ??
                        new HashSet<string>();

                    deviceProperties.Tags.UnionWith(devicePropertiesFromStorage.Tags);
                    deviceProperties.Reported.UnionWith(devicePropertiesFromStorage.Reported);
                    etag = model.ETag;
                    // If the new set of deviceProperties are already there in cache, return
                    if (deviceProperties.Tags.Count == devicePropertiesFromStorage.Tags.Count &&
                        deviceProperties.Reported.Count == devicePropertiesFromStorage.Reported.Count)
                    {
                        return deviceProperties;
                    }
                }

                var value = JsonConvert.SerializeObject(deviceProperties);
                try
                {
                    var response = await this.storageClient.UpdateAsync(
                        CacheCollectioId, CacheKey, value, etag);
                    return JsonConvert.DeserializeObject<DevicePropertyServiceModel>(response.Data);
                }
                catch (ConflictingResourceException)
                {
                    logger.LogInformation("Cache updating: failed due to conflict. Retry soon");
                }
                catch (Exception e)
                {
                    logger.LogInformation(e, "Cache updating: failed");
                    throw new Exception("Cache updating: failed");
                }
            }
        }

        private static void ParseWhitelist(
            string whitelist,
            out DeviceTwinName fullNameWhitelist,
            out DeviceTwinName prefixWhitelist)
        {
            /// <example>
            /// whitelist = "tags.*, reported.Protocol, reported.SupportedMethods,
            ///                 reported.DeviceMethodStatus, reported.FirmwareUpdateStatus"
            /// whitelistItems = [tags.*,
            ///                   reported.Protocol,
            ///                   reported.SupportedMethods,
            ///                   reported.DeviceMethodStatus,
            ///                   reported.FirmwareUpdateStatus]
            /// </example>
            var whitelistItems = whitelist.Split(',').Select(s => s.Trim());

            /// <example>
            /// tags = [tags.*]
            /// </example>
            var tags = whitelistItems
                .Where(s => s.StartsWith(WhitelistTagPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Substring(WhitelistTagPrefix.Length));

            /// <example>
            /// reported = [reported.Protocol,
            ///             reported.SupportedMethods,
            ///             reported.DeviceMethodStatus,
            ///             reported.FirmwareUpdateStatus]
            /// </example>
            var reported = whitelistItems
                .Where(s => s.StartsWith(WhitelistReportedPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Substring(WhitelistReportedPrefix.Length));

            /// <example>
            /// fixedTags = []
            /// </example>
            var fixedTags = tags.Where(s => !s.EndsWith("*"));
            /// <example>
            /// fixedReported = [reported.Protocol,
            ///                  reported.SupportedMethods,
            ///                  reported.DeviceMethodStatus,
            ///                  reported.FirmwareUpdateStatus]
            /// </example>
            var fixedReported = reported.Where(s => !s.EndsWith("*"));

            /// <example>
            /// regexTags = [tags.]
            /// </example>
            var regexTags = tags.Where(s => s.EndsWith("*")).Select(s => s.Substring(0, s.Length - 1));
            /// <example>
            /// regexReported = []
            /// </example>
            var regexReported = reported.
                Where(s => s.EndsWith("*")).
                Select(s => s.Substring(0, s.Length - 1));

            /// <example>
            /// fullNameWhitelist = {Tags = [],
            ///                      ReportedProperties = [
            ///                         reported.Protocol,
            ///                         reported.SupportedMethods,
            ///                         reported.DeviceMethodStatus,
            ///                         reported.FirmwareUpdateStatus]
            ///                      }
            /// </example>
            fullNameWhitelist = new DeviceTwinName
            {
                Tags = new HashSet<string>(fixedTags),
                ReportedProperties = new HashSet<string>(fixedReported),
            };

            /// <example>
            /// prefixWhitelist = {Tags = [tags.],
            ///                    ReportedProperties = []}
            /// </example>
            prefixWhitelist = new DeviceTwinName
            {
                Tags = new HashSet<string>(regexTags),
                ReportedProperties = new HashSet<string>(regexReported),
            };
        }

        private async Task<DeviceTwinName> GetValidNamesAsync()
        {
            ParseWhitelist(this.whitelist, out var fullNameWhitelist, out var prefixWhitelist);

            var validNames = new DeviceTwinName
            {
                Tags = fullNameWhitelist.Tags,
                ReportedProperties = fullNameWhitelist.ReportedProperties,
            };

            if (prefixWhitelist.Tags.Any() || prefixWhitelist.ReportedProperties.Any())
            {
                DeviceTwinName allNames = new DeviceTwinName();
                try
                {
                    // Get list of DeviceTwinNames from IOT-hub
                    allNames = await this.devices.GetDeviceTwinNamesAsync();
                }
                catch (Exception e)
                {
                    throw new ExternalDependencyException("Unable to fetch IoT devices", e);
                }

                validNames.Tags.UnionWith(allNames.Tags.
                    Where(s => prefixWhitelist.Tags.Any(s.StartsWith)));

                validNames.ReportedProperties.UnionWith(
                    allNames.ReportedProperties.Where(
                        s => prefixWhitelist.ReportedProperties.Any(s.StartsWith)));
            }

            return validNames;
        }

        private bool ShouldCacheRebuild(bool force, ValueApiModel valueApiModel)
        {
            if (force)
            {
                logger.LogInformation("Cache will be rebuilt due to the force flag");
                return true;
            }

            if (valueApiModel == null)
            {
                logger.LogInformation("Cache will be rebuilt since no cache was found");
                return true;
            }

            DevicePropertyServiceModel cacheValue = new DevicePropertyServiceModel();
            DateTimeOffset timstamp;
            try
            {
                cacheValue = JsonConvert.DeserializeObject<DevicePropertyServiceModel>(valueApiModel.Data);
                timstamp = DateTimeOffset.Parse(valueApiModel.Metadata["$modified"]);
            }
            catch
            {
                logger.LogInformation("DeviceProperties will be rebuilt because the last one is broken.");
                return true;
            }

            if (cacheValue.Rebuilding)
            {
                if (timstamp.AddSeconds(this.rebuildTimeout) < DateTimeOffset.UtcNow)
                {
                    logger.LogDebug("Cache will be rebuilt because last rebuilding had timedout");
                    return true;
                }
                else
                {
                    logger.LogDebug("Cache rebuilding skipped because it is being rebuilt by other instance");
                    return false;
                }
            }
            else
            {
                if (cacheValue.IsNullOrEmpty())
                {
                    logger.LogInformation("Cache will be rebuilt since it is empty");
                    return true;
                }

                if (timstamp.AddSeconds(this.ttl) < DateTimeOffset.UtcNow)
                {
                    logger.LogInformation("Cache will be rebuilt because it has expired");
                    return true;
                }
                else
                {
                    logger.LogDebug("Cache rebuilding skipped because it has not expired");
                    return false;
                }
            }
        }
    }
}
