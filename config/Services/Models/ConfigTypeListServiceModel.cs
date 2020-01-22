// <copyright file="ConfigTypeListServiceModel.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Mmm.Iot.Config.Services.Models;
using Newtonsoft.Json;

namespace Mmm.Iot.Config.Services.External
{
    public class ConfigTypeListServiceModel
    {
        private HashSet<string> configTypes = new HashSet<string>();

        [JsonProperty("configtypes")]
        public string[] ConfigTypes
        {
            get
            {
                return configTypes.ToArray<string>();
            }

            set
            {
                Array.ForEach<string>(value, c => configTypes.Add(c));
            }
        }

        internal void Add(string customConfig)
        {
            configTypes.Add(customConfig.Trim());
        }
    }
}