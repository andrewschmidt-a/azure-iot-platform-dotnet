// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    public class ActionApiModel
    {
        public ActionApiModel(string type, Dictionary<string, object> parameters)
        {
            Type = type;

            try
            {
                Parameters = new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                var msg = $"Error, duplicate parameters provided for the {Type} action. " +
                          "Parameters are case-insensitive.";
                throw new InvalidInputException(msg, e);
            }
        }

        public ActionApiModel(IAction action)
        {
            Type = action.Type.ToString();
            Parameters = action.Parameters;
        }

        public ActionApiModel()
        {
            Type = ActionType.Email.ToString();
            Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        [JsonProperty(PropertyName = "Type")]
        public string Type { get; }

        // Note: Parameters dictionary should always be initialized as case-insensitive.
        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<string, object> Parameters { get; }

        public IAction ToServiceModel()
        {
            if (!Enum.TryParse(Type, true, out ActionType action))
            {
                var validActionsList = string.Join(", ", Enum.GetNames(typeof(ActionType)).ToList());
                throw new InvalidInputException($"The action type '{Type}' is not valid." +
                                                $"Valid action types: [{validActionsList}]");
            }

            switch (action)
            {
                case ActionType.Email:
                    return new EmailAction(Parameters);
                default:
                    var validActionsList = string.Join(", ", Enum.GetNames(typeof(ActionType)).ToList());
                    throw new InvalidInputException($"The action type '{Type}' is not valid" +
                                                    $"Valid action types: [{validActionsList}]");
            }
        }
    }
}
