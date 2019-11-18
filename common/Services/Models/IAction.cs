using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    /// <summary>
    /// Interface for all Actions that can be added as part of a Rule.
    /// New action types should implement IAction and be added to the ActionType enum.
    /// Parameters should be a case-insensitive dictionary used to pass additional
    /// information required for any given action type.
    /// </summary>
    [JsonConverter(typeof(ActionConverter))]
    public interface IAction
    {
        [JsonConverter(typeof(StringEnumConverter))]
        ActionType Type { get; }

        // Note: Parameters should always be initialized as a case-insensitive dictionary
        IDictionary<string, object> Parameters { get; }
    }
}