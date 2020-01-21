using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Models
{
    public class RuleListApiModel
    {
        private List<RuleApiModel> items;

        public RuleListApiModel(List<Rule> rules, bool includeDeleted)
        {
            this.items = new List<RuleApiModel>();
            if (rules != null)
            {
                foreach (Rule rule in rules)
                {
                    this.items.Add(new RuleApiModel(rule, includeDeleted));
                }
            }
        }

        [JsonProperty(PropertyName = "Items")]
        public List<RuleApiModel> Items
        {
            get { return this.items; }
        }

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public IDictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", "RuleList;1" },
            { "$uri", "/" + "v1/rules" },
        };
    }
}
