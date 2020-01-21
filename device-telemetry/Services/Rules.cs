using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.DeviceTelemetry.Services.External;
using Mmm.Platform.IoT.DeviceTelemetry.Services.Helpers;
using Mmm.Platform.IoT.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services
{
    public class Rules : IRules
    {
        public const string STORAGE_COLLECTION = "rules";
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";
        private readonly IStorageAdapterClient _storage;
        private readonly ILogger _logger;
        private readonly IAsaManagerClient _asaManager;
        private readonly IAlarms _alarms;
        private readonly IDiagnosticsClient _diagnosticsClient;

        public Rules(
            IStorageAdapterClient storage,
            IAsaManagerClient asaManager,
            ILogger<Rules> logger,
            IAlarms alarms,
            IDiagnosticsClient diagnosticsClient)
        {
            this._storage = storage;
            this._asaManager = asaManager;
            this._logger = logger;
            this._alarms = alarms;
            this._diagnosticsClient = diagnosticsClient;
        }

        public async Task CreateFromTemplateAsync(string template)
        {
            string pathToTemplate = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                @"Data\Rules\" + template + ".json");

            if (RuleTemplateValidator.IsValid(pathToTemplate))
            {
                var file = JToken.Parse(File.ReadAllText(pathToTemplate));

                foreach (var item in file["Rules"])
                {
                    Rule newRule = this.Deserialize(item.ToString());

                    await this.CreateAsync(newRule);
                }
            }
        }

        public async Task DeleteAsync(string id)
        {
            InputValidator.Validate(id);

            Rule existing;
            try
            {
                existing = await this.GetAsync(id);
            }
            catch (ResourceNotFoundException exception)
            {
                _logger.LogDebug(exception, "Tried to delete rule {id} which did not exist", id);
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error trying to delete rule {id}", id);
                throw exception;
            }

            if (existing.Deleted)
            {
                return;
            }

            existing.Deleted = true;

            var item = JsonConvert.SerializeObject(existing);
            await this._storage.UpdateAsync(
                STORAGE_COLLECTION,
                existing.Id,
                item,
                existing.ETag);
            await this._asaManager.BeginConversionAsync(STORAGE_COLLECTION);
            LogEventAndRuleCountToDiagnostics("Rule_Deleted");
        }

        public async Task<Rule> GetAsync(string id)
        {
            InputValidator.Validate(id);

            var item = await this._storage.GetAsync(STORAGE_COLLECTION, id);
            var rule = this.Deserialize(item.Data);

            rule.ETag = item.ETag;
            rule.Id = item.Key;

            return rule;
        }

        public async Task<List<Rule>> GetListAsync(
            string order,
            int skip,
            int limit,
            string groupId,
            bool includeDeleted)
        {
            InputValidator.Validate(order);
            if (!string.IsNullOrEmpty(groupId))
            {
                InputValidator.Validate(groupId);
            }

            var data = await this._storage.GetAllAsync(STORAGE_COLLECTION);
            var ruleList = new List<Rule>();
            foreach (var item in data.Items)
            {
                try
                {
                    var rule = this.Deserialize(item.Data);
                    rule.ETag = item.ETag;
                    rule.Id = item.Key;

                    if ((string.IsNullOrEmpty(groupId) ||
                        rule.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
                        && (!rule.Deleted || includeDeleted))
                    {
                        ruleList.Add(rule);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "Could not parse result from Key Value Storage");
                    throw new InvalidDataException(
                        "Could not parse result from Key Value Storage", e);
                }
            }

            // sort based on MessageTime, default descending
            ruleList.Sort();

            if (order.Equals("asc", StringComparison.OrdinalIgnoreCase))
            {
                ruleList.Reverse();
            }

            if (skip >= ruleList.Count)
            {
                _logger.LogDebug("Skip value {skip} greater than size of list returned ({ruleListCount})", skip, ruleList.Count);

                return new List<Rule>();
            }
            else if ((limit + skip) >= ruleList.Count)
            {
                // if requested values are out of range, return remaining items
                return ruleList.GetRange(skip, ruleList.Count - skip);
            }

            return ruleList.GetRange(skip, limit);
        }

        public async Task<List<AlarmCountByRule>> GetAlarmCountForListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            InputValidator.Validate(order);
            foreach (var device in devices)
            {
                InputValidator.Validate(device);
            }

            var alarmCountByRuleList = new List<AlarmCountByRule>();

            // get list of rules
            var rulesList = await this.GetListAsync(order, skip, limit, null, true);

            // get open alarm count and most recent alarm for each rule
            foreach (var rule in rulesList)
            {
                var alarmCount = await this._alarms.GetCountByRuleAsync(
                    rule.Id,
                    from,
                    to,
                    devices);

                // skip to next rule if no alarms found
                if (alarmCount == 0)
                {
                    continue;
                }

                // get most recent alarm for rule
                var recentAlarm = await this.GetLastAlarmForRuleAsync(rule.Id, from, to, devices);

                // add alarmCountByRule to list
                alarmCountByRuleList.Add(
                    new AlarmCountByRule(
                        alarmCount,
                        recentAlarm.Status,
                        recentAlarm.DateCreated,
                        rule));
            }

            return alarmCountByRuleList;
        }

        public async Task<Rule> CreateAsync(Rule rule)
        {
            if (rule == null)
            {
                throw new InvalidInputException("Rule not provided.");
            }
            rule.Validate();

            // Ensure dates are correct
            rule.DateCreated = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
            rule.DateModified = rule.DateCreated;

            var item = JsonConvert.SerializeObject(rule);
            var result = await this._storage.CreateAsync(STORAGE_COLLECTION, item);
            await this._asaManager.BeginConversionAsync(STORAGE_COLLECTION);

            Rule newRule = this.Deserialize(result.Data);
            newRule.ETag = result.ETag;

            if (string.IsNullOrEmpty(newRule.Id)) newRule.Id = result.Key;
            LogEventAndRuleCountToDiagnostics("Rule_Created");

            return newRule;
        }

        public async Task<Rule> UpsertIfNotDeletedAsync(Rule rule)
        {
            rule.Validate();

            if (rule == null)
            {
                throw new InvalidInputException("Rule not provided.");
            }

            // Ensure dates are correct
            // Get the existing rule so we keep the created date correct
            Rule savedRule = null;
            try
            {
                savedRule = await this.GetAsync(rule.Id);
            }
            catch (ResourceNotFoundException e)
            {
                // Following the pattern of Post should create or update
                _logger.LogInformation(e, "Rule not found will create new rule for ID {ruleId}", rule.Id);
            }

            if (savedRule != null && savedRule.Deleted)
            {
                throw new ResourceNotFoundException($"Rule {rule.Id} not found");
            }

            return await this.UpdateAsync(rule, savedRule);
        }

        private async void LogEventAndRuleCountToDiagnostics(string eventName)
        {
            if (this._diagnosticsClient.CanLogToDiagnostics)
            {
                await this._diagnosticsClient.LogEventAsync(eventName);
                int ruleCount = await this.GetRuleCountAsync();
                var eventProperties = new Dictionary<string, object>
                {
                    { "Count", ruleCount }
                };
                await this._diagnosticsClient.LogEventAsync("Rule_Count", eventProperties);
            }
        }

        private async Task<Rule> UpdateAsync(Rule rule, Rule savedRule)
        {
            // If rule does not exist and id is provided upsert rule with that id
            if (savedRule == null && rule.Id != null)
            {
                rule.DateCreated = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
                rule.DateModified = rule.DateCreated;
            }
            else
            {
                rule.DateCreated = savedRule.DateCreated;
                rule.DateModified = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
            }

            // Save the updated rule if it exists or create new rule with id
            var item = JsonConvert.SerializeObject(rule);
            var result = await this._storage.UpdateAsync(
                STORAGE_COLLECTION,
                rule.Id,
                item,
                rule.ETag);
            await this._asaManager.BeginConversionAsync(STORAGE_COLLECTION);

            Rule updatedRule = this.Deserialize(result.Data);

            updatedRule.ETag = result.ETag;
            updatedRule.Id = result.Key;

            return updatedRule;
        }

        private async Task<Alarm> GetLastAlarmForRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string[] devices)
        {
            var resultList = await this._alarms.ListByRuleAsync(
                id,
                from,
                to,
                "desc",
                0,
                1,
                devices);

            if (resultList.Count != 0)
            {
                return resultList[0];
            }
            else
            {
                _logger.LogDebug("Could not retrieve most recent alarm {id}", id);
                throw new ExternalDependencyException(
                    "Could not retrieve most recent alarm");
            }
        }

        private Rule Deserialize(string jsonRule)
        {
            try
            {
                return JsonConvert.DeserializeObject<Rule>(jsonRule);
            }
            catch (Exception e)
            {
                throw new ExternalDependencyException("Unable to parse data.", e);
            }
        }

        private async Task<int> GetRuleCountAsync()
        {
            ValueListApiModel rules = await this._storage.GetAllAsync(STORAGE_COLLECTION);
            int ruleCount = 0;
            foreach (var item in rules.Items)
            {
                var rule = this.Deserialize(item.Data);
                if (!rule.Deleted)
                {
                    ruleCount++;
                }
            }

            return ruleCount;
        }
    }
}
