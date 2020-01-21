// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Mmm.Platform.IoT.Config.Services.Models.Actions;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Config.WebService.v1.Models
{
    public class ActionSettingsListApiModel
    {
        public ActionSettingsListApiModel(List<IActionSettings> actionSettingsList)
        {
            this.Items = new List<ActionSettingsApiModel>();

            foreach (var actionSettings in actionSettingsList)
            {
                this.Items.Add(new ActionSettingsApiModel(actionSettings));
            }

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"ActionSettingsList;1" },
                { "$url", $"/v1/solution-settings/actions" }
            };
        }

        [JsonProperty("Items")]
        public List<ActionSettingsApiModel> Items { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}
