// <copyright file="AppConfigurationSource.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Mmm.Platform.IoT.Common.Services
{
    public class AppConfigurationSource : IConfigurationSource
    {
        private readonly string appConfigConnectionString;
        private readonly List<string> appConfigurationKeys;

        public AppConfigurationSource(string appConfigConnectionString, List<string> appConfigurationKeys)
        {
            this.appConfigConnectionString = appConfigConnectionString;
            this.appConfigurationKeys = appConfigurationKeys;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AppConfigurationProvider(this.appConfigConnectionString, this.appConfigurationKeys);
        }
    }
}