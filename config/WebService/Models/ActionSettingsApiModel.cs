using System;
using System.Collections.Generic;
using Mmm.Platform.IoT.Config.Services.Models.Actions;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Config.WebService.Models
{
    public class ActionSettingsApiModel
    {
        public ActionSettingsApiModel()
        {
            this.Type = ActionType.Email.ToString();
            this.Settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public ActionSettingsApiModel(IActionSettings actionSettings)
        {
            this.Type = actionSettings.Type.ToString();

            this.Settings = new Dictionary<string, object>(
                actionSettings.Settings,
                StringComparer.OrdinalIgnoreCase);
        }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Settings")]
        public IDictionary<string, object> Settings { get; set; }
    }
}
