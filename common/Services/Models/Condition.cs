using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    public class Condition
    {
        public Condition() { }

        public string Field { get; set; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public OperatorType Operator { get; set; }

        public string Value { get; set; } = string.Empty;
    }
}
