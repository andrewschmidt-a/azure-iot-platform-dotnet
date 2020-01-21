using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Config.Services.External;
using Mmm.Platform.IoT.Config.Services.Helpers;
using Mmm.Platform.IoT.Config.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Config.Services
{
    public class Seed : ISeed
    {
        private const string SEED_COLLECTION_ID = "solution-settings";
        private const string MUTEX_KEY = "seedMutex";
        private const string COMPLETED_FLAG_KEY = "seedCompleted";
        private readonly TimeSpan mutexTimeout = TimeSpan.FromMinutes(5);

        private readonly AppConfig config;
        private readonly IStorageMutex mutex;
        private readonly IStorage storage;
        private readonly IStorageAdapterClient storageClient;
        private readonly IDeviceSimulationClient simulationClient;
        private readonly IDeviceTelemetryClient telemetryClient;
        private readonly ILogger _logger;

        public Seed(
            AppConfig config,
            IStorageMutex mutex,
            IStorage storage,
            IStorageAdapterClient storageClient,
            IDeviceSimulationClient simulationClient,
            IDeviceTelemetryClient telemetryClient,
            ILogger<Seed> logger)
        {
            this.config = config;
            this.mutex = mutex;
            this.storage = storage;
            this.storageClient = storageClient;
            this.simulationClient = simulationClient;
            this.telemetryClient = telemetryClient;
            _logger = logger;
        }

        public async Task TrySeedAsync()
        {
            if (string.IsNullOrEmpty(config.ConfigService.SeedTemplate))
            {
                return;
            }

            if (!await this.mutex.EnterAsync(SEED_COLLECTION_ID, MUTEX_KEY, this.mutexTimeout))
            {
                _logger.LogInformation("Seed skipped (conflict)");
                return;
            }

            if (await this.CheckCompletedFlagAsync())
            {
                _logger.LogInformation("Seed skipped (completed)");
                return;
            }

            _logger.LogInformation("Seed begin");
            try
            {
                await this.SeedAsync();
                _logger.LogInformation("Seed end");
                await this.SetCompletedFlagAsync();
                _logger.LogInformation("Seed completed flag set");
            }
            finally
            {
                await this.mutex.LeaveAsync(SEED_COLLECTION_ID, MUTEX_KEY);
            }
        }

        private async Task<bool> CheckCompletedFlagAsync()
        {
            try
            {
                await this.storageClient.GetAsync(SEED_COLLECTION_ID, COMPLETED_FLAG_KEY);
                return true;
            }
            catch (ResourceNotFoundException)
            {
                return false;
            }
        }

        private async Task SetCompletedFlagAsync()
        {
            await this.storageClient.UpdateAsync(SEED_COLLECTION_ID, COMPLETED_FLAG_KEY, "true", "*");
        }

        private async Task SeedAsync()
        {
            if (config.ConfigService.SolutionType.StartsWith("devicesimulation", StringComparison.OrdinalIgnoreCase))
            {
                await this.SeedSimulationAsync();
            }
            else
            {
                await this.SeedSingleTemplateAsync();
            }
        }

        // Seed single template for Remote Monitoring solution
        private async Task SeedSingleTemplateAsync()
        {
            var template = this.GetSeedContent(config.ConfigService.SeedTemplate);

            if (template.Groups.Select(g => g.Id).Distinct().Count() != template.Groups.Count())
            {
                _logger.LogWarning("Found duplicated group ID {groups}", template.Groups);
            }

            if (template.Rules.Select(r => r.Id).Distinct().Count() != template.Rules.Count())
            {
                _logger.LogWarning("Found duplicated rule ID {rules}", template.Rules);
            }

            var groupIds = new HashSet<string>(template.Groups.Select(g => g.Id));
            var rulesWithInvalidGroupId = template.Rules.Where(r => !groupIds.Contains(r.GroupId));
            if (rulesWithInvalidGroupId.Any())
            {
                _logger.LogWarning("Invalid group ID found in rules {rules}", rulesWithInvalidGroupId);
            }

            foreach (var group in template.Groups)
            {
                try
                {
                    await this.storage.UpdateDeviceGroupAsync(group.Id, group, "*");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to seed default group {group}", group);
                    throw;
                }
            }

            foreach (var rule in template.Rules)
            {
                try
                {
                    await this.telemetryClient.UpdateRuleAsync(rule, "*");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to seed default rule {rule}", rule);
                    throw;
                }
            }

            await this.SeedSimulationAsync();
        }

        // Seed single template for Device Simulation solution
        private async Task SeedSimulationAsync()
        {
            try
            {
                var simulationModel = await this.simulationClient.GetDefaultSimulationAsync();

                if (simulationModel != null)
                {
                    _logger.LogInformation("Skip seed simulation since there is already one simulation {simulationModel}", simulationModel);
                }
                else
                {
                    var template = this.GetSeedContent(config.ConfigService.SeedTemplate);

                    foreach (var simulation in template.Simulations)
                    {
                        await this.simulationClient.UpdateSimulationAsync(simulation);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed default simulations");
                throw;
            }
        }

        private Template GetSeedContent(string templateName)
        {
            Template template;
            string content;
            var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var file = Path.Combine(root, "Data", $"{templateName}.json");
            if (!File.Exists(file))
            {
                // ToDo: Check if `template` is a valid URL and try to load the content

                throw new ResourceNotFoundException($"Template {templateName} does not exist");
            }
            else
            {
                content = File.ReadAllText(file);
            }

            try
            {
                template = JsonConvert.DeserializeObject<Template>(content);
            }
            catch (Exception ex)
            {
                throw new InvalidInputException("Failed to parse template", ex);
            }

            return template;
        }
    }
}